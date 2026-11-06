import fs from "fs";
import path from "path";
import { AirsAnimal, AirsVaccination, VaccinationStatus } from "./types";

const DATA_DIR = process.env.DATA_DIR ?? path.join(process.cwd(), "data");
const SEED_DIR = process.env.SEED_DIR ?? path.join(process.cwd(), "seed");

const ANIMALS_FILE = path.join(DATA_DIR, "airs-animals.json");
const VACCINATIONS_FILE = path.join(DATA_DIR, "airs-vaccinations.json");

let animals: AirsAnimal[] = [];
let vaccinations: AirsVaccination[] = [];

function ensureDataFiles(): void {
  fs.mkdirSync(DATA_DIR, { recursive: true });

  if (!fs.existsSync(ANIMALS_FILE)) {
    fs.copyFileSync(path.join(SEED_DIR, "airs-animals.seed.json"), ANIMALS_FILE);
  }

  if (!fs.existsSync(VACCINATIONS_FILE)) {
    fs.copyFileSync(path.join(SEED_DIR, "airs-vaccinations.seed.json"), VACCINATIONS_FILE);
  }
}

function readJson<T>(filePath: string): T {
  return JSON.parse(fs.readFileSync(filePath, "utf8")) as T;
}

function writeJson(filePath: string, value: unknown): void {
  fs.writeFileSync(filePath, JSON.stringify(value, null, 2), "utf8");
}

export function initStore(): void {
  ensureDataFiles();
  animals = readJson<AirsAnimal[]>(ANIMALS_FILE);
  vaccinations = readJson<AirsVaccination[]>(VACCINATIONS_FILE);
}

export function computeVaccinationStatus(nextDueAt: string | null): VaccinationStatus {
  if (!nextDueAt) {
    return "Unknown";
  }

  const due = new Date(`${nextDueAt}T00:00:00Z`);
  if (Number.isNaN(due.getTime())) {
    return "Unknown";
  }

  const today = new Date();
  const todayUtc = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());
  const dueUtc = Date.UTC(due.getUTCFullYear(), due.getUTCMonth(), due.getUTCDate());
  const daysUntilDue = Math.floor((dueUtc - todayUtc) / 86_400_000);

  if (daysUntilDue < 0) {
    return "Expired";
  }

  if (daysUntilDue <= 60) {
    return "DueSoon";
  }

  return "Valid";
}

function normalizeVaccination(vaccination: AirsVaccination): AirsVaccination {
  return {
    ...vaccination,
    status: computeVaccinationStatus(vaccination.nextDueAt)
  };
}

export function getAnimals(): AirsAnimal[] {
  return animals;
}

export function getAnimalById(id: string): AirsAnimal | undefined {
  return animals.find((animal) => animal.id === id);
}

export function getAnimalByMicrochip(microchipNumber: string): AirsAnimal | undefined {
  return animals.find((animal) => animal.microchipNumber === microchipNumber);
}

export function getVaccinationsByAnimalId(animalId: string): AirsVaccination[] {
  return vaccinations.filter((item) => item.animalId === animalId).map(normalizeVaccination);
}

export function getVaccinationsByMicrochip(microchipNumber: string): AirsVaccination[] {
  return vaccinations
    .filter((item) => item.microchipNumber === microchipNumber)
    .map(normalizeVaccination);
}

export function getAllVaccinations(): AirsVaccination[] {
  return vaccinations.map(normalizeVaccination);
}

export function addAnimal(animal: AirsAnimal, newVaccinations: AirsVaccination[]): void {
  animals.push(animal);
  vaccinations.push(...newVaccinations);
  writeJson(ANIMALS_FILE, animals);
  writeJson(VACCINATIONS_FILE, vaccinations);
}

export function updateAnimal(animal: AirsAnimal): void {
  const index = animals.findIndex((item) => item.id === animal.id);
  if (index < 0) {
    throw new Error("Animal not found");
  }

  animals[index] = animal;
  writeJson(ANIMALS_FILE, animals);
}

export function addVaccination(vaccination: AirsVaccination): void {
  vaccinations.push(vaccination);
  writeJson(VACCINATIONS_FILE, vaccinations);
}

export function getCounts(): { animals: number; vaccinations: number } {
  return { animals: animals.length, vaccinations: vaccinations.length };
}
