import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, "..");
const seedDir = path.join(root, "seed");
fs.mkdirSync(seedDir, { recursive: true });

function mulberry32(seed) {
  let value = seed;
  return function random() {
    value |= 0;
    value = (value + 0x6d2b79f5) | 0;
    let t = Math.imul(value ^ (value >>> 15), 1 | value);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const rng = mulberry32(20261103);
const pick = (items) => items[Math.floor(rng() * items.length)];
const randInt = (min, max) => Math.floor(rng() * (max - min + 1)) + min;

const today = new Date();
today.setUTCHours(0, 0, 0, 0);

const pad = (value) => String(value).padStart(2, "0");
const toDateOnly = (date) =>
  `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}`;

function addDays(date, days) {
  const result = new Date(date);
  result.setUTCDate(result.getUTCDate() + days);
  return result;
}

function addYears(date, years) {
  const result = new Date(date);
  result.setUTCFullYear(result.getUTCFullYear() + years);
  return result;
}

function randomDateBetween(start, end) {
  const startMs = start.getTime();
  const endMs = end.getTime();
  if (endMs <= startMs) {
    return new Date(endMs);
  }
  return new Date(startMs + Math.floor(rng() * (endMs - startMs)));
}

function vaccinationStatus(nextDueAt) {
  const days = Math.floor((nextDueAt.getTime() - today.getTime()) / 86_400_000);
  if (days < 0) return "Expired";
  if (days <= 60) return "DueSoon";
  return "Valid";
}

const firstNames = [
  "გიორგი", "ნინო", "დავით", "მარიამ", "ლევან", "ანა", "ირაკლი", "თამარ", "საბა", "ელენე",
  "ლუკა", "სოფიო", "ზურაბ", "ქეთევან", "ალექსანდრე", "ნათია", "მიხეილ", "ლალი", "ვაჟა", "მაია",
  "ბექა", "სალომე", "გიგა", "ანი", "ოთარ", "მანანა", "რატი", "თეონა", "ზაზა", "ირმა"
];

const lastNames = [
  "ჯღარკავა", "ბერიძე", "კაპანაძე", "გელაშვილი", "მაისურაძე", "ხუციშვილი", "წიკლაური",
  "ჩხეიძე", "ლომიძე", "სულაბერიძე", "ფხაკაძე", "ღვინიაშვილი", "მეგრელიშვილი", "კიკნაძე",
  "ნოზაძე", "შარაშიძე", "ბახტაძე", "ჯავახიშვილი", "აბულაძე", "ჩიხლაძე"
];

const cities = ["თბილისი", "ბათუმი", "ქუთაისი", "რუსთავი", "გორი", "ზუგდიდი", "თელავი", "ფოთი"];
const streets = [
  "ვაჟა-ფშაველას გამზირი", "ყაზბეგის გამზირი", "მერაბ კოსტავას ქუჩა", "აღმაშენებლის გამზირი",
  "ჭავჭავაძის გამზირი", "თამარ მეფის ქუჩა", "გორგასლის ქუჩა", "დავით აღმაშენებლის ქუჩა"
];

const dogNames = ["ბობი", "რექსი", "ლუკა", "თომა", "ბარსი", "მაქსი", "ჯეკი", "შერი", "ლესი", "დიზელი"];
const catNames = ["მურა", "ლუსი", "ტომი", "ბელა", "მიმი", "სიმბა", "ნალა", "ფელიქსი", "კიტი", "ზუზუ"];
const otherNames = ["კურდღელი", "ზაზუ", "პუპუ", "ჩიკო", "მანჩო"];

const dogBreeds = [
  "German Shepherd", "Labrador Retriever", "Golden Retriever", "Husky", "Poodle",
  "French Bulldog", "Border Collie", "Caucasian Shepherd", "Beagle", "Mixed"
];
const catBreeds = ["British Shorthair", "Persian", "Maine Coon", "Siamese", "Scottish Fold", "Mixed"];
const otherBreeds = ["Rabbit", "Parrot", "Guinea Pig", "Hamster", "Mixed"];

const colors = ["შავი", "თეთრი", "ყავისფერი", "ნაცრისფერი", "წითური", "ხასიათი", "სამფერი", "კრემისფერი"];
const marks = [
  "",
  "თეთრი ლაქა მკერდზე",
  "შავი ლაქა ყურზე",
  "დაზიანებული მარცხენა ყური",
  "თეთრი წინდები",
  "კუდის ბოლოში თეთრი ბუმბული",
  "პატარა ნაწიბური მარჯვენა თვალთან"
];

const vetNames = ["ნინო ბერიძე", "გიორგი მაისურაძე", "თამარ კაპანაძე", "ლევან ჩხეიძე", "ანა გელაშვილი"];
const clinics = ["ვეტკლინიკა პეტვეტი", "ვეტკლინიკა ფაუნა", "ვეტკლინიკა აიბოლიტი", "ვეტკლინიკა ზოოვეტი"];
const manufacturers = ["MSD Animal Health", "Zoetis", "Boehringer Ingelheim", "Virbac", "Elanco"];

const vaccineCatalog = {
  Dog: [
    { vaccineName: "Rabies", disease: "ცოფი" },
    { vaccineName: "DHPP", disease: "დისტემპერი, ჰეპატიტი, პარვოვირუსი, პარაგრიპი" },
    { vaccineName: "Leptospirosis", disease: "ლეპტოსპიროზი" },
    { vaccineName: "Bordetella", disease: "ბორდეტელოზი" },
    { vaccineName: "Canine Influenza", disease: "ძაღლების გრიპი" }
  ],
  Cat: [
    { vaccineName: "Rabies", disease: "ცოფი" },
    { vaccineName: "FVRCP", disease: "კატების ვირუსული რინოტრაქეიტი, კალიცივირუსი, პანლეიკოპენია" },
    { vaccineName: "Feline Leukemia", disease: "კატების ლეიკემია" },
    { vaccineName: "Bordetella", disease: "ბორდეტელოზი" }
  ],
  Other: [{ vaccineName: "Rabies", disease: "ცოფი" }]
};

const owners = [];
for (let i = 0; i < 300; i += 1) {
  const city = pick(cities);
  owners.push({
    personalId: String(1000000001 + i).padStart(11, "0"),
    firstName: firstNames[i % firstNames.length],
    lastName: lastNames[(i * 7) % lastNames.length],
    phone: `+9955${String(10000000 + i * 137).slice(-8)}`,
    email: `owner${i + 1}@example.com`,
    address: `${city}, ${pick(streets)} ${randInt(1, 120)}`
  });
}

const animals = [];
const vaccinations = [];
let vaccinationSequence = 1;

for (let i = 1; i <= 1000; i += 1) {
  const speciesRoll = rng();
  const species = speciesRoll < 0.55 ? "Dog" : speciesRoll < 0.95 ? "Cat" : "Other";
  const name = species === "Dog" ? pick(dogNames) : species === "Cat" ? pick(catNames) : pick(otherNames);
  const breed = species === "Dog" ? pick(dogBreeds) : species === "Cat" ? pick(catBreeds) : pick(otherBreeds);
  const birthDate = randomDateBetween(
    new Date(Date.UTC(2010, 0, 1)),
    new Date(Date.UTC(2025, 11, 1))
  );
  const statusRoll = rng();
  const status =
    statusRoll < 0.9 ? "Active" : statusRoll < 0.95 ? "Lost" : statusRoll < 0.98 ? "Found" : "Deceased";
  const registeredAt = randomDateBetween(new Date(Date.UTC(2024, 0, 1)), today);
  const updatedAt = addDays(registeredAt, randInt(0, 120));
  const owner = owners[(i - 1) % owners.length];

  const animal = {
    id: `animal-${String(i).padStart(6, "0")}`,
    microchipNumber: `982${String(i).padStart(12, "0")}`,
    name,
    species,
    breed,
    gender: rng() < 0.5 ? "Male" : "Female",
    birthDate: toDateOnly(birthDate),
    color: pick(colors),
    distinguishingMarks: pick(marks),
    status,
    owner,
    registeredAt: registeredAt.toISOString(),
    updatedAt: updatedAt.toISOString()
  };

  animals.push(animal);

  const vaccineCount = randInt(1, 4);
  for (let j = 0; j < vaccineCount; j += 1) {
    const catalog = vaccineCatalog[species];
    const vaccine = catalog[(j + randInt(0, catalog.length - 1)) % catalog.length];
    const earliest = new Date(Math.max(addDays(birthDate, 120).getTime(), addDays(today, -1095).getTime()));
    const administeredAt = randomDateBetween(earliest, today);
    const nextDueAt = addYears(administeredAt, 1);

    vaccinations.push({
      id: `vac-${String(vaccinationSequence).padStart(7, "0")}`,
      animalId: animal.id,
      microchipNumber: animal.microchipNumber,
      vaccineName: vaccine.vaccineName,
      disease: vaccine.disease,
      manufacturer: pick(manufacturers),
      batchNumber: `BATCH-${administeredAt.getUTCFullYear()}-${String(randInt(1, 9999)).padStart(4, "0")}`,
      administeredAt: toDateOnly(administeredAt),
      nextDueAt: toDateOnly(nextDueAt),
      veterinarianName: pick(vetNames),
      clinicName: pick(clinics),
      certificateNumber: `CERT-${administeredAt.getUTCFullYear()}-${String(vaccinationSequence).padStart(6, "0")}`,
      status: vaccinationStatus(nextDueAt)
    });

    vaccinationSequence += 1;
  }
}

fs.writeFileSync(path.join(seedDir, "airs-animals.seed.json"), JSON.stringify(animals, null, 2));
fs.writeFileSync(path.join(seedDir, "airs-vaccinations.seed.json"), JSON.stringify(vaccinations, null, 2));

console.log(`Generated ${animals.length} animals and ${vaccinations.length} vaccinations.`);
console.log(`First microchip: ${animals[0].microchipNumber} (owner ${animals[0].owner.personalId})`);
