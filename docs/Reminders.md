# Vaccination reminders

HappyPet sends vaccination reminders by email. The feature is implemented in the API process; no external scheduler is required.

## Configuration

Add a `Reminders` section to the API configuration:

```json
{
  "Reminders": {
    "Enabled": true,
    "RunHourLocal": 9,
    "Timezone": "Asia/Tbilisi",
    "MaxAttempts": 3,
    "OverdueLookbackDays": 30,
    "Offsets": [ 30, 7, 1, 0 ]
  }
}
```

| Key | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Starts the daily background job. `appsettings.Development.json` sets it to `false` to prevent accidental real emails. |
| `RunHourLocal` | `9` | Local hour (0-23) when the daily job runs. |
| `Timezone` | `Asia/Tbilisi` | Timezone used for the run hour. Falls back to `Georgian Standard Time`, then UTC. |
| `MaxAttempts` | `3` | Maximum delivery attempts for a failed reminder. |
| `OverdueLookbackDays` | `30` | How far back overdue vaccinations are considered when no reminder exists yet. |
| `Offsets` | `[30, 7, 1, 0]` | Reminder stages in days before the due date. The closest applicable stage is used. |

Email delivery uses the existing `Email` section (`SmtpHost`, `SmtpPort`, `EnableSsl`, `From`, `SmtpUser`, `SmtpPassword`).

## Stages

For a vaccination due in 30 days, the `30` stage is sent. The same stage is used if the job runs later but the due date is still more than 7 days away. At 7 days the `7` stage is sent, at 1 day the `1` stage, and on the due date or later the `0` stage.

Only one stage is selected per run, so a missed job does not produce several old emails at once.

## Delivery rules

- Only live pets (`isDeceased = false`) with a non-null `nextDueAt` are considered.
- The owner must have `isEmailVerified = true` and a non-empty email address.
- A reminder row is unique by vaccination, due date, stage and channel.
- A `Sent` reminder is never delivered again for the same due date.
- A failed delivery is retried while `attemptCount < MaxAttempts`.
- A failed attempt stores the SMTP error in `lastError`.
- If the pet owner changes, the next unsent stage uses the new owner's email.
- If `nextDueAt` changes, the new due date gets a new reminder set; history for the old due date is retained.
- Deleting a vaccination cascades to its reminder rows.
- Unverified owners are skipped and no reminder row is created, so a later verification can still enable the reminder.

## Email content

The email is plain text in Georgian. It contains:

- the owner's first name;
- the pet name;
- the vaccine name;
- the due date;
- the number of remaining or overdue days.

No diagnosis, dosage or medical advice is included.

## API

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/reminders` | authenticated | Current user's reminder history, paginated |
| GET | `/api/v1/pet/{id}/reminders` | owner or admin | Reminder history for one pet, paginated |
| POST | `/api/v1/reminders/run` | admin | Runs the reminder job immediately and returns counts |

Example manual run response:

```json
{
  "runAt": "2026-11-15T09:00:00Z",
  "candidates": 12,
  "sent": 10,
  "failed": 1,
  "skipped": 1,
  "retried": 2
}
```

`POST /api/v1/reminders/run` works even when `Reminders:Enabled` is `false`; it is intended for smoke tests and operational recovery.

## History response

Each history item contains:

- `id`, `vaccinationId`, `petId`;
- `petName`, `vaccineName`;
- `dueAt`, `offsetDays`, `scheduledFor`;
- `channel` (`Email`);
- `status` (`Pending`, `Sent`, `Failed`, `Skipped`);
- `sentAt`, `attemptCount`, `lastError`, `skipReason`;
- `createdAt`.

## Current limitations

- Only email is supported; SMS and mobile push are not implemented.
- The job lives inside the API process, so it runs only while the API is running.
- Real SMTP delivery must be configured and verified separately.
- The scheduler uses UTC-normalized dates for due comparisons; the run hour is the only timezone-sensitive part.
