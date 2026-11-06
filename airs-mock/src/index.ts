import crypto from "crypto";
import cors from "cors";
import express from "express";
import morgan from "morgan";
import swaggerUi from "swagger-ui-express";
import { openApiDocument } from "./openapi";
import {
  addAnimal,
  addVaccination,
  computeVaccinationStatus,
  getAnimalById,
  getAnimalByMicrochip,
  getAnimals,
  getAllVaccinations,
  getCounts,
  getVaccinationsByAnimalId,
  getVaccinationsByMicrochip,
  initStore,
  updateAnimal
} from "./store";
import { AirsAnimal, AirsOwner, AirsVaccination, AnimalStatus } from "./types";

const app = express();
const port = Number(process.env.PORT ?? 9100);
const expectedApiKey = process.env.AIRS_API_KEY;

app.use(cors());
app.use(express.json({ limit: "2mb" }));
app.use(morgan("dev"));

if (expectedApiKey) {
  app.use("/api/v1/airs", (req, res, next) => {
    if (req.header("X-Api-Key") !== expectedApiKey) {
      return res.status(401).json({
        code: "AIRS_UNAUTHORIZED",
        message: "AIRS API key არასწორია ან არ არის მითითებული."
      });
    }

    next();
  });
}

const animalStatuses: AnimalStatus[] = ["Active", "Lost", "Found", "Deceased"];

function notFoundAnimal(res: express.Response) {
  return res.status(404).json({
    code: "AIRS_ANIMAL_NOT_FOUND",
    message: "ცხოველი მითითებული მიკროჩიპით AIRS სისტემაში ვერ მოიძებნა."
  });
}

function validationError(res: express.Response, message: string) {
  return res.status(400).json({
    code: "AIRS_VALIDATION_ERROR",
    message
  });
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === "string" && value.trim().length > 0;
}

function isMicrochip(value: unknown): value is string {
  return typeof value === "string" && /^\d{15}$/.test(value);
}

function normalizeStatus(value: unknown, fallback: AnimalStatus = "Active"): AnimalStatus {
  return animalStatuses.includes(value as AnimalStatus) ? (value as AnimalStatus) : fallback;
}

function buildVaccination(
  animal: Pick<AirsAnimal, "id" | "microchipNumber">,
  input: Partial<AirsVaccination>
): AirsVaccination {
  const administeredAt = input.administeredAt ?? new Date().toISOString().slice(0, 10);
  const nextDueAt =
    input.nextDueAt ??
    (() => {
      const date = new Date(`${administeredAt}T00:00:00Z`);
      date.setUTCFullYear(date.getUTCFullYear() + 1);
      return date.toISOString().slice(0, 10);
    })();

  return {
    id: input.id ?? crypto.randomUUID(),
    animalId: animal.id,
    microchipNumber: animal.microchipNumber,
    vaccineName: input.vaccineName ?? "Rabies",
    disease: input.disease ?? "ცოფი",
    manufacturer: input.manufacturer ?? "Unknown",
    batchNumber: input.batchNumber ?? `BATCH-${new Date().getFullYear()}-${Math.floor(Math.random() * 9000 + 1000)}`,
    administeredAt,
    nextDueAt,
    veterinarianName: input.veterinarianName ?? "Unknown",
    clinicName: input.clinicName ?? "Unknown",
    certificateNumber:
      input.certificateNumber ?? `CERT-${new Date().getFullYear()}-${Math.floor(Math.random() * 900000 + 100000)}`,
    status: computeVaccinationStatus(nextDueAt)
  };
}

function animalDetails(animal: AirsAnimal) {
  return {
    animal,
    vaccinations: getVaccinationsByAnimalId(animal.id)
  };
}

app.get("/health", (_req, res) => {
  const counts = getCounts();
  res.json({
    status: "ok",
    service: "happypet-airs-mock",
    animals: counts.animals,
    vaccinations: counts.vaccinations
  });
});

app.get("/openapi.json", (_req, res) => {
  res.json(openApiDocument);
});

app.use("/swagger", swaggerUi.serve, swaggerUi.setup(openApiDocument));

app.get("/api/v1/airs/animals", (req, res) => {
  const page = Math.max(1, Number(req.query.page) || 1);
  const pageSize = Math.min(200, Math.max(1, Number(req.query.pageSize) || 50));
  const q = String(req.query.q ?? "").trim().toLowerCase();
  const species = String(req.query.species ?? "").trim().toLowerCase();
  const status = String(req.query.status ?? "").trim().toLowerCase();
  const ownerPersonalId = String(req.query.ownerPersonalId ?? "").trim();

  let items = getAnimals();

  if (q) {
    items = items.filter((animal) => {
      const haystack = [
        animal.name,
        animal.microchipNumber,
        animal.breed,
        animal.owner.firstName,
        animal.owner.lastName,
        animal.owner.personalId
      ]
        .join(" ")
        .toLowerCase();
      return haystack.includes(q);
    });
  }

  if (species) {
    items = items.filter((animal) => animal.species.toLowerCase() === species);
  }

  if (status) {
    items = items.filter((animal) => animal.status.toLowerCase() === status);
  }

  if (ownerPersonalId) {
    items = items.filter((animal) => animal.owner.personalId === ownerPersonalId);
  }

  const total = items.length;
  const start = (page - 1) * pageSize;
  const paged = items.slice(start, start + pageSize);

  res.json({ page, pageSize, total, items: paged });
});

app.get("/api/v1/airs/animals/by-microchip/:microchip", (req, res) => {
  const animal = getAnimalByMicrochip(req.params.microchip.trim());

  if (!animal) {
    return notFoundAnimal(res);
  }

  res.json(animalDetails(animal));
});

app.get("/api/v1/airs/animals/:id", (req, res) => {
  const animal = getAnimalById(req.params.id);

  if (!animal) {
    return notFoundAnimal(res);
  }

  res.json(animalDetails(animal));
});

app.get("/api/v1/airs/animals/:id/vaccinations", (req, res) => {
  const animal = getAnimalById(req.params.id);

  if (!animal) {
    return notFoundAnimal(res);
  }

  res.json(getVaccinationsByAnimalId(animal.id));
});

app.post("/api/v1/airs/animals", (req, res) => {
  const body = req.body ?? {};

  if (!isMicrochip(body.microchipNumber)) {
    return validationError(res, "microchipNumber უნდა შედგებოდეს 15 ციფრისგან.");
  }

  if (!isNonEmptyString(body.name) || !isNonEmptyString(body.species)) {
    return validationError(res, "name და species სავალდებულო ველებია.");
  }

  const owner = body.owner as Partial<AirsOwner> | undefined;
  if (
    !owner ||
    !isNonEmptyString(owner.personalId) ||
    !isNonEmptyString(owner.firstName) ||
    !isNonEmptyString(owner.lastName)
  ) {
    return validationError(res, "owner.personalId, owner.firstName და owner.lastName სავალდებულო ველებია.");
  }

  if (getAnimalByMicrochip(body.microchipNumber)) {
    return res.status(409).json({
      code: "AIRS_MICROCHIP_ALREADY_EXISTS",
      message: "მითითებული მიკროჩიპის ნომერი AIRS სისტემაში უკვე რეგისტრირებულია."
    });
  }

  const now = new Date().toISOString();
  const animal: AirsAnimal = {
    id: crypto.randomUUID(),
    microchipNumber: body.microchipNumber,
    name: body.name.trim(),
    species: body.species.trim(),
    breed: isNonEmptyString(body.breed) ? body.breed.trim() : "Unknown",
    gender: isNonEmptyString(body.gender) ? body.gender.trim() : "Unknown",
    birthDate: isNonEmptyString(body.birthDate) ? body.birthDate : null,
    color: isNonEmptyString(body.color) ? body.color.trim() : "",
    distinguishingMarks: isNonEmptyString(body.distinguishingMarks)
      ? body.distinguishingMarks.trim()
      : "",
    status: normalizeStatus(body.status),
    owner: {
      personalId: owner.personalId.trim(),
      firstName: owner.firstName.trim(),
      lastName: owner.lastName.trim(),
      phone: isNonEmptyString(owner.phone) ? owner.phone.trim() : "",
      email: isNonEmptyString(owner.email) ? owner.email.trim() : "",
      address: isNonEmptyString(owner.address) ? owner.address.trim() : ""
    },
    registeredAt: now,
    updatedAt: now
  };

  const newVaccinations: AirsVaccination[] = Array.isArray(body.vaccinations)
    ? body.vaccinations.map((item: Partial<AirsVaccination>) => buildVaccination(animal, item))
    : [];

  addAnimal(animal, newVaccinations);

  return res.status(201).json({
    animal,
    vaccinations: newVaccinations.map((item) => ({
      ...item,
      status: computeVaccinationStatus(item.nextDueAt)
    }))
  });
});

app.put("/api/v1/airs/animals/:id", (req, res) => {
  const animal = getAnimalById(req.params.id);

  if (!animal) {
    return notFoundAnimal(res);
  }

  const body = req.body ?? {};
  const nextMicrochip = isNonEmptyString(body.microchipNumber)
    ? body.microchipNumber.trim()
    : animal.microchipNumber;

  if (!isMicrochip(nextMicrochip)) {
    return validationError(res, "microchipNumber უნდა შედგებოდეს 15 ციფრისგან.");
  }

  const duplicate = getAnimalByMicrochip(nextMicrochip);
  if (duplicate && duplicate.id !== animal.id) {
    return res.status(409).json({
      code: "AIRS_MICROCHIP_ALREADY_EXISTS",
      message: "მითითებული მიკროჩიპის ნომერი AIRS სისტემაში უკვე რეგისტრირებულია."
    });
  }

  const owner = (body.owner ?? {}) as Partial<AirsOwner>;
  const updated: AirsAnimal = {
    ...animal,
    microchipNumber: nextMicrochip,
    name: isNonEmptyString(body.name) ? body.name.trim() : animal.name,
    species: isNonEmptyString(body.species) ? body.species.trim() : animal.species,
    breed: isNonEmptyString(body.breed) ? body.breed.trim() : animal.breed,
    gender: isNonEmptyString(body.gender) ? body.gender.trim() : animal.gender,
    birthDate: body.birthDate === null ? null : isNonEmptyString(body.birthDate) ? body.birthDate : animal.birthDate,
    color: isNonEmptyString(body.color) ? body.color.trim() : animal.color,
    distinguishingMarks: isNonEmptyString(body.distinguishingMarks)
      ? body.distinguishingMarks.trim()
      : animal.distinguishingMarks,
    status: body.status ? normalizeStatus(body.status, animal.status) : animal.status,
    owner: {
      ...animal.owner,
      personalId: isNonEmptyString(owner.personalId) ? owner.personalId.trim() : animal.owner.personalId,
      firstName: isNonEmptyString(owner.firstName) ? owner.firstName.trim() : animal.owner.firstName,
      lastName: isNonEmptyString(owner.lastName) ? owner.lastName.trim() : animal.owner.lastName,
      phone: isNonEmptyString(owner.phone) ? owner.phone.trim() : animal.owner.phone,
      email: isNonEmptyString(owner.email) ? owner.email.trim() : animal.owner.email,
      address: isNonEmptyString(owner.address) ? owner.address.trim() : animal.owner.address
    },
    updatedAt: new Date().toISOString()
  };

  updateAnimal(updated);

  res.json(animalDetails(updated));
});

app.post("/api/v1/airs/animals/:id/status", (req, res) => {
  const animal = getAnimalById(req.params.id);

  if (!animal) {
    return notFoundAnimal(res);
  }

  if (!animalStatuses.includes(req.body?.status)) {
    return validationError(res, "status უნდა იყოს Active, Lost, Found ან Deceased.");
  }

  const updated = {
    ...animal,
    status: req.body.status as AnimalStatus,
    updatedAt: new Date().toISOString()
  };

  updateAnimal(updated);
  res.json(animalDetails(updated));
});

app.get("/api/v1/airs/vaccinations", (req, res) => {
  const microchip = String(req.query.microchip ?? "").trim();
  const animalId = String(req.query.animalId ?? "").trim();

  if (microchip) {
    return res.json(getVaccinationsByMicrochip(microchip));
  }

  if (animalId) {
    return res.json(getVaccinationsByAnimalId(animalId));
  }

  return res.json(getAllVaccinations());
});

app.post("/api/v1/airs/vaccinations", (req, res) => {
  const body = req.body ?? {};
  const microchip = String(body.microchipNumber ?? "").trim();
  const animal = microchip
    ? getAnimalByMicrochip(microchip)
    : isNonEmptyString(body.animalId)
      ? getAnimalById(body.animalId)
      : undefined;

  if (!animal) {
    return notFoundAnimal(res);
  }

  if (!isNonEmptyString(body.vaccineName) || !isNonEmptyString(body.administeredAt)) {
    return validationError(res, "vaccineName და administeredAt სავალდებულო ველებია.");
  }

  const vaccination = buildVaccination(animal, body);
  addVaccination(vaccination);

  res.status(201).json({
    ...vaccination,
    status: computeVaccinationStatus(vaccination.nextDueAt)
  });
});

app.use((err: unknown, _req: express.Request, res: express.Response, _next: express.NextFunction) => {
  console.error(err);
  res.status(500).json({
    code: "AIRS_INTERNAL_ERROR",
    message: "AIRS mock-ში მოხდა შიდა შეცდომა."
  });
});

initStore();
app.listen(port, "0.0.0.0", () => {
  const counts = getCounts();
  console.log(`HappyPet AIRS mock listening on http://0.0.0.0:${port}`);
  console.log(`Animals: ${counts.animals}, vaccinations: ${counts.vaccinations}`);
});
