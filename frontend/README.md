# Campus Placement Management — Angular Test UI

A minimal, lightweight Angular UI designed for testing and verifying all 15 backend API modules.

## How to Run

1. Make sure the backend Web API is running:
   ```bash
   dotnet run --project Campus_Management/Campus_Management.csproj
   ```
   *(Listening on `http://localhost:5214`)*

2. Start the Angular UI:
   ```bash
   cd frontend
   npm start
   ```

3. Open your browser at:
   ```
   http://localhost:4200
   ```

## Features Included
- **1-Click Quick Role Login**: Admin, Company HR, Student, plus custom credentials.
- **Reports & Dashboard**: Visual KPI cards for platform stats, student placement summaries, and company hiring metrics.
- **Colleges & Companies**: Browse colleges and registered employers.
- **Jobs & Interview Rounds**: Explore published openings and their configured interview rounds.
- **Applications & Status History**: View applications, transition statuses (Shortlist, Reject, Withdraw), and inspect the immutable status history audit log.
- **Interview Schedules**: Track scheduled interviews, interviewers, and score/feedback.
- **Interactive Live API Console**: Send arbitrary GET/POST/PUT/DELETE requests with JWT bearer authentication and view JSON responses.
