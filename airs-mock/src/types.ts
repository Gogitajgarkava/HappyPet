export type AnimalStatus = "Active" | "Lost" | "Found" | "Deceased";
export type VaccinationStatus = "Valid" | "Expired" | "DueSoon" | "Unknown";

export interface AirsOwner {
  personalId: string;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  address: string;
}

export interface AirsAnimal {
  id: string;
  microchipNumber: string;
  name: string;
  species: string;
  breed: string;
  gender: string;
  birthDate: string | null;
  color: string;
  distinguishingMarks: string;
  status: AnimalStatus;
  owner: AirsOwner;
  registeredAt: string;
  updatedAt: string;
}

export interface AirsVaccination {
  id: string;
  animalId: string;
  microchipNumber: string;
  vaccineName: string;
  disease: string;
  manufacturer: string;
  batchNumber: string;
  administeredAt: string;
  nextDueAt: string | null;
  veterinarianName: string;
  clinicName: string;
  certificateNumber: string;
  status: VaccinationStatus;
}

export interface AirsAnimalDetails {
  animal: AirsAnimal;
  vaccinations: AirsVaccination[];
}
