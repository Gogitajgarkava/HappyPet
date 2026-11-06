export const openApiDocument = {
  openapi: "3.0.3",
  info: {
    title: "HappyPet AIRS Mock API",
    version: "1.0.0",
    description:
      "Development-only mock of the Georgian AIRS animal registry. It stores data in JSON files and supports animal and vaccination lookup, registration and persistence."
  },
  servers: [
    {
      url: "http://localhost:9100",
      description: "Local Docker stack"
    }
  ],
  components: {
    securitySchemes: {
      ApiKeyAuth: {
        type: "apiKey",
        in: "header",
        name: "X-Api-Key"
      }
    },
    schemas: {
      Owner: {
        type: "object",
        required: ["personalId", "firstName", "lastName"],
        properties: {
          personalId: { type: "string", example: "01000000001" },
          firstName: { type: "string", example: "გიორგი" },
          lastName: { type: "string", example: "ჯღარკავა" },
          phone: { type: "string", example: "+995555123456" },
          email: { type: "string", example: "owner1@example.com" },
          address: { type: "string", example: "თბილისი, ვაჟა-ფშაველას გამზირი 12" }
        }
      },
      Animal: {
        type: "object",
        properties: {
          id: { type: "string", example: "animal-000001" },
          microchipNumber: { type: "string", example: "982000000000001" },
          name: { type: "string", example: "ბობი" },
          species: { type: "string", enum: ["Dog", "Cat", "Other"], example: "Dog" },
          breed: { type: "string", example: "German Shepherd" },
          gender: { type: "string", enum: ["Male", "Female", "Unknown"], example: "Male" },
          birthDate: { type: "string", nullable: true, example: "2021-04-12" },
          color: { type: "string", example: "შავი" },
          distinguishingMarks: { type: "string", example: "თეთრი ლაქა მკერდზე" },
          status: {
            type: "string",
            enum: ["Active", "Lost", "Found", "Deceased"],
            example: "Active"
          },
          owner: { $ref: "#/components/schemas/Owner" },
          registeredAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" }
        }
      },
      Vaccination: {
        type: "object",
        properties: {
          id: { type: "string", example: "vac-0000001" },
          animalId: { type: "string", example: "animal-000001" },
          microchipNumber: { type: "string", example: "982000000000001" },
          vaccineName: { type: "string", example: "Rabies" },
          disease: { type: "string", example: "ცოფი" },
          manufacturer: { type: "string", example: "MSD Animal Health" },
          batchNumber: { type: "string", example: "BATCH-2025-0412" },
          administeredAt: { type: "string", format: "date", example: "2025-04-12" },
          nextDueAt: { type: "string", format: "date", nullable: true, example: "2026-04-12" },
          veterinarianName: { type: "string", example: "ნინო ბერიძე" },
          clinicName: { type: "string", example: "ვეტკლინიკა პეტვეტი" },
          certificateNumber: { type: "string", example: "CERT-2025-000412" },
          status: {
            type: "string",
            enum: ["Valid", "Expired", "DueSoon", "Unknown"],
            example: "Valid"
          }
        }
      },
      AnimalDetails: {
        type: "object",
        properties: {
          animal: { $ref: "#/components/schemas/Animal" },
          vaccinations: {
            type: "array",
            items: { $ref: "#/components/schemas/Vaccination" }
          }
        }
      },
      Error: {
        type: "object",
        properties: {
          code: { type: "string", example: "AIRS_ANIMAL_NOT_FOUND" },
          message: { type: "string", example: "ცხოველი მითითებული მიკროჩიპით AIRS სისტემაში ვერ მოიძებნა." }
        }
      }
    }
  },
  security: [{ ApiKeyAuth: [] }],
  paths: {
    "/health": {
      get: {
        summary: "Health check",
        security: [],
        responses: {
          "200": { description: "Service is healthy" }
        }
      }
    },
    "/api/v1/airs/animals": {
      get: {
        summary: "List and search animals",
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", default: 1 } },
          { name: "pageSize", in: "query", schema: { type: "integer", default: 50, maximum: 200 } },
          { name: "q", in: "query", description: "Name, microchip or owner name", schema: { type: "string" } },
          { name: "species", in: "query", schema: { type: "string" } },
          { name: "status", in: "query", schema: { type: "string" } },
          { name: "ownerPersonalId", in: "query", schema: { type: "string" } }
        ],
        responses: {
          "200": { description: "Paged animal list" }
        }
      },
      post: {
        summary: "Register a new animal in AIRS",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: {
                type: "object",
                required: ["microchipNumber", "name", "species", "owner"],
                properties: {
                  microchipNumber: { type: "string", example: "982000000009999" },
                  name: { type: "string", example: "ლუკა" },
                  species: { type: "string", example: "Dog" },
                  breed: { type: "string", example: "Husky" },
                  gender: { type: "string", example: "Male" },
                  birthDate: { type: "string", example: "2023-01-20" },
                  color: { type: "string", example: "თეთრი" },
                  distinguishingMarks: { type: "string" },
                  status: { type: "string", example: "Active" },
                  owner: { $ref: "#/components/schemas/Owner" },
                  vaccinations: {
                    type: "array",
                    items: { $ref: "#/components/schemas/Vaccination" }
                  }
                }
              }
            }
          }
        },
        responses: {
          "201": { description: "Animal registered" },
          "409": { description: "Microchip already exists" }
        }
      }
    },
    "/api/v1/airs/animals/by-microchip/{microchip}": {
      get: {
        summary: "Look up an animal and its vaccinations by microchip",
        parameters: [
          {
            name: "microchip",
            in: "path",
            required: true,
            schema: { type: "string", example: "982000000000001" }
          }
        ],
        responses: {
          "200": {
            description: "Animal and vaccinations",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/AnimalDetails" }
              }
            }
          },
          "404": {
            description: "Animal not found",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/Error" }
              }
            }
          }
        }
      }
    },
    "/api/v1/airs/animals/{id}": {
      get: {
        summary: "Get an animal by AIRS id",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string" } }],
        responses: {
          "200": { description: "Animal and vaccinations" },
          "404": { description: "Animal not found" }
        }
      },
      put: {
        summary: "Update an animal",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string" } }],
        responses: {
          "200": { description: "Updated animal" },
          "404": { description: "Animal not found" }
        }
      }
    },
    "/api/v1/airs/animals/{id}/vaccinations": {
      get: {
        summary: "Get vaccinations for an animal",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string" } }],
        responses: {
          "200": { description: "Vaccination list" },
          "404": { description: "Animal not found" }
        }
      }
    },
    "/api/v1/airs/animals/{id}/status": {
      post: {
        summary: "Update animal status",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: {
                type: "object",
                required: ["status"],
                properties: {
                  status: {
                    type: "string",
                    enum: ["Active", "Lost", "Found", "Deceased"]
                  }
                }
              }
            }
          }
        },
        responses: {
          "200": { description: "Updated animal" },
          "404": { description: "Animal not found" }
        }
      }
    },
    "/api/v1/airs/vaccinations": {
      get: {
        summary: "Get vaccinations by microchip or animal id",
        parameters: [
          { name: "microchip", in: "query", schema: { type: "string" } },
          { name: "animalId", in: "query", schema: { type: "string" } }
        ],
        responses: {
          "200": { description: "Vaccination list" }
        }
      },
      post: {
        summary: "Add a vaccination",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: {
                type: "object",
                required: ["microchipNumber", "vaccineName", "administeredAt"],
                properties: {
                  microchipNumber: { type: "string" },
                  animalId: { type: "string" },
                  vaccineName: { type: "string" },
                  disease: { type: "string" },
                  manufacturer: { type: "string" },
                  batchNumber: { type: "string" },
                  administeredAt: { type: "string", format: "date" },
                  nextDueAt: { type: "string", format: "date" },
                  veterinarianName: { type: "string" },
                  clinicName: { type: "string" },
                  certificateNumber: { type: "string" }
                }
              }
            }
          }
        },
        responses: {
          "201": { description: "Vaccination added" },
          "404": { description: "Animal not found" }
        }
      }
    }
  }
};
