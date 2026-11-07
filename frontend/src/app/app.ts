import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <!-- STATE 1: NOT LOGGED IN -> DEDICATED LOGIN PAGE -->
    <div *ngIf="!token()" style="min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f8fafc; padding: 20px;">
      <div style="width: 100%; max-width: 440px; background: white; border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px -5px rgba(0,0,0,0.05); padding: 32px;">
        <div style="text-align: center; margin-bottom: 24px;">
          <div style="font-size: 40px; line-height: 1; margin-bottom: 12px;">🎓</div>
          <h1 style="font-size: 22px; font-weight: 700; color: #0f172a;">Campus Management</h1>
          <p style="font-size: 13px; color: #64748b; margin-top: 4px;">Sign in to test and verify the placement system</p>
        </div>

        <!-- Manual Login Form -->
        <form (submit)="login(); $event.preventDefault()">
          <div style="margin-bottom: 14px;">
            <label style="font-size: 12px; font-weight: 600; color: #334155; display: block; margin-bottom: 4px;">Email Address</label>
            <input [(ngModel)]="loginEmail" name="email" type="email" placeholder="e.g. admin@gmail.com" style="width: 100%;" required />
          </div>

          <div style="margin-bottom: 18px;">
            <label style="font-size: 12px; font-weight: 600; color: #334155; display: block; margin-bottom: 4px;">Password</label>
            <input [(ngModel)]="loginPassword" name="password" type="password" placeholder="••••••••" style="width: 100%;" required />
          </div>

          <button type="submit" class="btn-primary" style="width: 100%; padding: 10px; font-size: 14px;" [disabled]="authLoading()">
            {{ authLoading() ? 'Authenticating...' : 'Sign In' }}
          </button>
        </form>

        <div *ngIf="authError()" style="margin-top: 14px; padding: 10px 14px; background: #fee2e2; color: #dc2626; border-radius: 6px; font-size: 12px; text-align: center;">
          {{ authError() }}
        </div>

        <!-- 1-Click Role Login for Quick Testing -->
        <div style="margin-top: 24px; padding-top: 20px; border-top: 1px solid #e2e8f0;">
          <div style="font-size: 11px; font-weight: 700; color: #64748b; text-transform: uppercase; margin-bottom: 10px; text-align: center; letter-spacing: 0.5px;">
            ⚡ 1-Click Role Sign In
          </div>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px;">
            <button type="button" class="btn-secondary btn-sm" (click)="quickLogin('admin@gmail.com', 'Admin@123')">
              👑 Admin
            </button>
            <button type="button" class="btn-secondary btn-sm" (click)="quickLogin('sarah.hr@tcs.com', 'HrPassword@123')">
              🏢 Company HR
            </button>
            <button type="button" class="btn-secondary btn-sm" (click)="quickLogin('arun.cse@nit.edu', 'Student@123')">
              🎓 Student
            </button>
          </div>
        </div>

        <div style="margin-top: 24px; text-align: center; border-top: 1px solid #f1f5f9; padding-top: 16px;">
          <a href="/scalar/v1" target="_blank" style="font-size: 12px; color: #2563eb; font-weight: 500;">
            📚 Open Scalar OpenAPI Documentation ↗
          </a>
        </div>
      </div>
    </div>

    <!-- STATE 2: LOGGED IN -> FULL TEST WORKSPACE -->
    <div *ngIf="token()" style="max-width: 1200px; margin: 0 auto; padding: 20px;">
      <!-- Header -->
      <header style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #e2e8f0; padding-bottom: 16px; margin-bottom: 20px;">
        <div>
          <h1 style="font-size: 20px; font-weight: 700; color: #0f172a; display: flex; align-items: center; gap: 8px;">
            🎓 Campus Management — Testing Workspace
          </h1>
          <p style="font-size: 12px; color: #64748b; margin-top: 2px;">
            API Server: <span style="font-family: monospace; color: #2563eb;">{{ apiUrl() }}</span>
          </p>
        </div>

        <!-- Auth Status & Controls -->
        <div style="display: flex; align-items: center; gap: 12px;">
          <a href="/scalar/v1" target="_blank" class="btn-secondary btn-sm" style="display: flex; align-items: center; gap: 4px;">
            📚 Scalar API Docs ↗
          </a>
          <div style="text-align: right;">
            <div style="font-weight: 600; font-size: 13px;">{{ currentUser()?.fullName }}</div>
            <div style="font-size: 11px; color: #64748b;">
              {{ currentUser()?.email }} · <span class="badge badge-purple">{{ userRole() }}</span>
            </div>
          </div>
          <button class="btn-danger btn-sm" (click)="logout()">Sign Out</button>
        </div>
      </header>

      <!-- Navigation Tabs -->
      <nav style="display: flex; gap: 8px; border-bottom: 1px solid #e2e8f0; margin-bottom: 20px; overflow-x: auto; padding-bottom: 4px;">
        <button [class]="activeTab() === 'dashboard' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('dashboard')">
          📊 Reports & Dashboard
        </button>
        <button [class]="activeTab() === 'colleges' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('colleges')">
          🏛️ Colleges
        </button>
        <button [class]="activeTab() === 'companies' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('companies')">
          🏢 Companies
        </button>
        <button [class]="activeTab() === 'jobs' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('jobs')">
          💼 Jobs & Rounds
        </button>
        <button [class]="activeTab() === 'applications' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('applications')">
          📝 Applications & History
        </button>
        <button [class]="activeTab() === 'interviews' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('interviews')">
          📅 Interview Schedules
        </button>
        <button [class]="activeTab() === 'api-console' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('api-console')">
          ⚡ API Console
        </button>
      </nav>

      <!-- TAB 1: REPORTS & DASHBOARD -->
      <section *ngIf="activeTab() === 'dashboard'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">System Overview & Analytics</h2>
          <button class="btn-secondary btn-sm" (click)="loadReports()">🔄 Refresh Reports</button>
        </div>

        <div *ngIf="platformReport()" class="card">
          <h3 style="font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 12px; text-transform: uppercase;">Platform Overview (Admin)</h3>
          <div class="stat-grid">
            <div class="stat-box">
              <div class="stat-value">{{ platformReport().totalColleges }}</div>
              <div class="stat-label">Total Colleges</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ platformReport().totalCompanies }}</div>
              <div class="stat-label">Total Companies</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ platformReport().totalStudents }}</div>
              <div class="stat-label">Students</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ platformReport().totalJobs }}</div>
              <div class="stat-label">Jobs Posted</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ platformReport().totalApplications }}</div>
              <div class="stat-label">Applications</div>
            </div>
            <div class="stat-box">
              <div class="stat-value" style="color: #16a34a;">{{ platformReport().overallPlacementRatePercentage }}%</div>
              <div class="stat-label">Placement Rate</div>
            </div>
            <div class="stat-box">
              <div class="stat-value" style="color: #0284c7;">₹{{ platformReport().highestPackageOffered | number }}</div>
              <div class="stat-label">Highest Package</div>
            </div>
          </div>
        </div>

        <!-- Student Placement Summary -->
        <div *ngIf="studentReport()" class="card">
          <h3 style="font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            My Placement Summary ({{ studentReport().studentName }})
          </h3>
          <div class="stat-grid">
            <div class="stat-box">
              <div class="stat-value">{{ studentReport().totalApplications }}</div>
              <div class="stat-label">Applied</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ studentReport().interviewCount }}</div>
              <div class="stat-label">Interviews</div>
            </div>
            <div class="stat-box">
              <div class="stat-value" style="color: #16a34a;">{{ studentReport().selectedCount }}</div>
              <div class="stat-label">Selected</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ studentReport().rejectedCount }}</div>
              <div class="stat-label">Rejected</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">
                <span [class]="studentReport().isPlaced ? 'badge badge-green' : 'badge badge-yellow'">
                  {{ studentReport().isPlaced ? 'PLACED' : 'IN PROCESS' }}
                </span>
              </div>
              <div class="stat-label">Placement Status</div>
            </div>
          </div>
        </div>

        <!-- Company Recruitment Report -->
        <div *ngIf="companyReport()" class="card">
          <h3 style="font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            Company Recruitment Overview ({{ companyReport().companyName }})
          </h3>
          <div class="stat-grid">
            <div class="stat-box">
              <div class="stat-value">{{ companyReport().totalJobsPosted }}</div>
              <div class="stat-label">Jobs Posted</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ companyReport().totalApplicationsReceived }}</div>
              <div class="stat-label">Applications</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ companyReport().totalInterviewed }}</div>
              <div class="stat-label">Interviewed</div>
            </div>
            <div class="stat-box">
              <div class="stat-value" style="color: #16a34a;">{{ companyReport().totalHired }}</div>
              <div class="stat-label">Hired</div>
            </div>
            <div class="stat-box">
              <div class="stat-value">{{ companyReport().totalDrivesConducted }}</div>
              <div class="stat-label">Drives</div>
            </div>
          </div>
        </div>
      </section>

      <!-- TAB 2: COLLEGES -->
      <section *ngIf="activeTab() === 'colleges'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Colleges Directory</h2>
          <div style="display: flex; gap: 8px;">
            <button *ngIf="userRole() === 'Admin'" class="btn-primary btn-sm" (click)="showAddCollegeModal.set(!showAddCollegeModal())">
              ➕ Add College
            </button>
            <button class="btn-secondary btn-sm" (click)="loadColleges()">🔄 Refresh</button>
          </div>
        </div>

        <!-- Add College Form Card (Admin Only) -->
        <div *ngIf="showAddCollegeModal() && userRole() === 'Admin'" class="card" style="background: #f8fafc; border-left: 4px solid #2563eb; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px; color: #0f172a;">Register New College</h3>
          <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; margin-bottom: 14px;">
            <div>
              <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">College Name *</label>
              <input [(ngModel)]="newCollegeName" placeholder="e.g. National Institute of Technology" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">City *</label>
              <input [(ngModel)]="newCollegeCity" placeholder="e.g. Trichy" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">Email</label>
              <input [(ngModel)]="newCollegeEmail" type="email" placeholder="e.g. info@nit.edu" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">Phone Number</label>
              <input [(ngModel)]="newCollegePhone" placeholder="e.g. +91 9876543210" style="width: 100%;" />
            </div>
          </div>
          <div style="margin-bottom: 14px;">
            <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">Address</label>
            <input [(ngModel)]="newCollegeAddress" placeholder="Full campus address" style="width: 100%;" />
          </div>

          <div style="display: flex; gap: 8px; align-items: center;">
            <button class="btn-primary" (click)="createCollege()" [disabled]="collegeSaving()">
              {{ collegeSaving() ? 'Saving College...' : 'Save College' }}
            </button>
            <button class="btn-secondary" (click)="showAddCollegeModal.set(false)">Cancel</button>
            <span *ngIf="collegeError()" style="color: #dc2626; font-size: 12px; font-weight: 500;">
              ⚠️ {{ collegeError() }}
            </span>
          </div>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>College Name</th>
                <th>City</th>
                <th>Address</th>
                <th>Status</th>
                <th *ngIf="userRole() === 'Admin'">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of colleges()">
                <td>#{{ c.collegeId }}</td>
                <td style="font-weight: 600;">{{ c.collegeName }}</td>
                <td>{{ c.city }}</td>
                <td>{{ c.address || '—' }}</td>
                <td>
                  <span class="badge badge-green">{{ c.status === 1 ? 'Active' : 'Pending' }}</span>
                </td>
                <td *ngIf="userRole() === 'Admin'">
                  <button class="btn-danger btn-sm" (click)="deleteCollege(c.collegeId)">Delete</button>
                </td>
              </tr>
              <tr *ngIf="colleges().length === 0">
                <td [attr.colspan]="userRole() === 'Admin' ? 6 : 5" style="text-align: center; color: #64748b; padding: 20px;">No colleges loaded.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 3: COMPANIES -->
      <section *ngIf="activeTab() === 'companies'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Registered Companies</h2>
          <button class="btn-secondary btn-sm" (click)="loadCompanies()">🔄 Refresh</button>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Company Name</th>
                <th>Industry</th>
                <th>Website</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of companies()">
                <td>#{{ c.companyId }}</td>
                <td style="font-weight: 600;">{{ c.companyName }}</td>
                <td>{{ c.industry }}</td>
                <td><a [href]="c.websiteUrl" target="_blank">{{ c.websiteUrl }}</a></td>
                <td>
                  <span class="badge badge-blue">{{ c.status === 1 ? 'Active' : 'Pending' }}</span>
                </td>
              </tr>
              <tr *ngIf="companies().length === 0">
                <td colspan="5" style="text-align: center; color: #64748b; padding: 20px;">No companies loaded.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 4: JOBS & ROUNDS -->
      <section *ngIf="activeTab() === 'jobs'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Job Openings & Interview Rounds</h2>
          <button class="btn-secondary btn-sm" (click)="loadJobs()">🔄 Refresh Jobs</button>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>Job ID</th>
                <th>Title</th>
                <th>Company</th>
                <th>Package</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let j of jobs()">
                <td>#{{ j.jobId }}</td>
                <td style="font-weight: 600;">{{ j.jobTitle }}</td>
                <td>{{ j.companyName }}</td>
                <td>₹{{ j.salaryPackage | number }}</td>
                <td>
                  <span [class]="j.status === 2 ? 'badge badge-green' : 'badge badge-yellow'">
                    {{ j.status === 2 ? 'Published' : 'Draft' }}
                  </span>
                </td>
                <td>
                  <button class="btn-secondary btn-sm" (click)="viewRounds(j.jobId)">View Rounds</button>
                  <button *ngIf="userRole() === 'Student'" class="btn-primary btn-sm" style="margin-left: 6px;" (click)="applyJob(j.jobId)">Apply</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Selected Job Rounds Panel -->
        <div *ngIf="selectedJobRounds() !== null" class="card" style="border-left: 4px solid #2563eb;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 10px;">
            Interview Rounds for Job #{{ currentJobId() }} ({{ selectedJobRounds()?.length }} Rounds)
          </h3>
          <div *ngIf="selectedJobRounds()?.length === 0" style="color: #64748b; font-size: 13px;">No rounds configured yet for this job.</div>
          <div *ngFor="let r of selectedJobRounds()" style="padding: 8px 12px; background: #f8fafc; border-radius: 6px; margin-bottom: 6px;">
            <div style="display: flex; justify-content: space-between;">
              <strong>Round {{ r.roundNumber }}: {{ r.roundName }}</strong>
              <span class="badge badge-blue">{{ r.roundType }}</span>
            </div>
            <p style="font-size: 12px; color: #64748b; margin-top: 2px;">{{ r.description }}</p>
          </div>
        </div>
      </section>

      <!-- TAB 5: APPLICATIONS & HISTORY -->
      <section *ngIf="activeTab() === 'applications'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Job Applications</h2>
          <div style="display: flex; gap: 8px;">
            <input *ngIf="userRole() === 'CompanyHR' || userRole() === 'Admin'" [(ngModel)]="filterJobId" placeholder="Filter by Job ID" style="width: 140px;" />
            <button class="btn-secondary btn-sm" (click)="loadApplications()">🔄 Load Applications</button>
          </div>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>App ID</th>
                <th>Job Title</th>
                <th>Candidate</th>
                <th>College</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let a of applications()">
                <td>#{{ a.applicationId }}</td>
                <td style="font-weight: 600;">{{ a.jobTitle }}</td>
                <td>{{ a.studentName }}</td>
                <td>{{ a.collegeName }}</td>
                <td>
                  <span class="badge" [ngClass]="getStatusBadge(a.status)">
                    {{ getStatusName(a.status) }}
                  </span>
                </td>
                <td style="display: flex; gap: 4px;">
                  <button class="btn-secondary btn-sm" (click)="loadHistory(a.applicationId)">History</button>
                  <ng-container *ngIf="userRole() === 'CompanyHR'">
                    <button class="btn-success btn-sm" (click)="shortlistApp(a.applicationId)" [disabled]="a.status >= 3">Shortlist</button>
                    <button class="btn-danger btn-sm" (click)="rejectApp(a.applicationId)" [disabled]="a.status >= 6">Reject</button>
                  </ng-container>
                  <ng-container *ngIf="userRole() === 'Student'">
                    <button class="btn-danger btn-sm" (click)="withdrawApp(a.applicationId)" [disabled]="a.status >= 6">Withdraw</button>
                  </ng-container>
                </td>
              </tr>
              <tr *ngIf="applications().length === 0">
                <td colspan="6" style="text-align: center; color: #64748b; padding: 20px;">No applications found.</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- History Timeline Modal / Panel -->
        <div *ngIf="currentHistory() !== null" class="card" style="border-left: 4px solid #16a34a;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
            <h3 style="font-size: 14px; font-weight: 600;">Status History for Application #{{ selectedAppId() }}</h3>
            <button class="btn-secondary btn-sm" (click)="currentHistory.set(null)">✕ Close</button>
          </div>
          <div *ngIf="currentHistory()?.length === 0" style="color: #64748b;">No history records found.</div>
          <div style="position: relative; border-left: 2px solid #e2e8f0; margin-left: 10px; padding-left: 16px;">
            <div *ngFor="let h of currentHistory()" style="margin-bottom: 14px;">
              <div style="font-weight: 600; font-size: 13px;">
                {{ h.fromStatusText ? h.fromStatusText + ' ➔ ' : '' }}
                <span class="badge badge-green">{{ h.toStatusText }}</span>
              </div>
              <div style="font-size: 11px; color: #64748b;">
                By {{ h.changedByName }} ({{ h.changedByRole }}) on {{ h.changedAt | date:'medium' }}
              </div>
              <p *ngIf="h.remarks" style="font-size: 12px; margin-top: 4px; background: #f8fafc; padding: 4px 8px; border-radius: 4px;">
                💬 {{ h.remarks }}
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- TAB 6: INTERVIEW SCHEDULES -->
      <section *ngIf="activeTab() === 'interviews'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Interview Schedules</h2>
          <button class="btn-secondary btn-sm" (click)="loadInterviews()">🔄 Refresh</button>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>Schedule ID</th>
                <th>Round</th>
                <th>Candidate</th>
                <th>Interviewer</th>
                <th>Scheduled Window</th>
                <th>Status</th>
                <th>Score & Feedback</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let s of schedules()">
                <td>#{{ s.scheduleId }}</td>
                <td><strong>Round {{ s.roundNumber }}:</strong> {{ s.roundName }}</td>
                <td>{{ s.studentName }}</td>
                <td>{{ s.interviewerName }}</td>
                <td>{{ s.scheduledStartTime | date:'short' }} – {{ s.scheduledEndTime | date:'shortTime' }}</td>
                <td>
                  <span class="badge" [ngClass]="s.status === 2 ? 'badge-green' : s.status === 1 ? 'badge-blue' : 'badge-yellow'">
                    {{ s.status === 1 ? 'Scheduled' : s.status === 2 ? 'Completed' : s.status === 3 ? 'Cancelled' : 'Rescheduled' }}
                  </span>
                </td>
                <td>
                  <span *ngIf="s.score" style="font-weight: 600; color: #16a34a;">{{ s.score }}/100</span>
                  <div *ngIf="s.feedback" style="font-size: 11px; color: #64748b;">{{ s.feedback }}</div>
                  <span *ngIf="!s.score && !s.feedback" style="color: #94a3b8;">Pending</span>
                </td>
              </tr>
              <tr *ngIf="schedules().length === 0">
                <td colspan="7" style="text-align: center; color: #64748b; padding: 20px;">No interview schedules found.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 7: API CONSOLE (RAW TESTER) -->
      <section *ngIf="activeTab() === 'api-console'">
        <div class="card">
          <h2 style="font-size: 16px; font-weight: 600; margin-bottom: 12px;">⚡ Live API Request Console</h2>
          <p style="font-size: 12px; color: #64748b; margin-bottom: 16px;">
            Directly execute any endpoint in the backend using the active JWT Bearer token.
          </p>

          <div style="display: flex; gap: 8px; margin-bottom: 12px;">
            <select [(ngModel)]="consoleMethod" style="width: 110px; font-weight: 700;">
              <option value="GET">GET</option>
              <option value="POST">POST</option>
              <option value="PUT">PUT</option>
              <option value="DELETE">DELETE</option>
            </select>
            <input [(ngModel)]="consoleEndpoint" placeholder="/api/reports/platform-overview" style="flex: 1; font-family: monospace;" />
            <button class="btn-primary" (click)="executeConsoleRequest()" [disabled]="consoleLoading()">
              {{ consoleLoading() ? 'Executing...' : 'Send Request' }}
            </button>
          </div>

          <div *ngIf="consoleMethod !== 'GET'" style="margin-bottom: 12px;">
            <label style="font-size: 12px; font-weight: 600; display: block; margin-bottom: 4px;">Request Payload (JSON):</label>
            <textarea [(ngModel)]="consoleBody" rows="4" style="width: 100%; font-family: monospace; font-size: 12px;"></textarea>
          </div>

          <div *ngIf="consoleResponse() !== null" style="margin-top: 16px;">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px;">
              <span style="font-size: 12px; font-weight: 600;">Response Output:</span>
              <span class="badge" [ngClass]="consoleStatus() >= 200 && consoleStatus() < 300 ? 'badge-green' : 'badge-red'">
                HTTP {{ consoleStatus() }}
              </span>
            </div>
            <pre style="background: #0f172a; color: #38bdf8; padding: 12px; border-radius: 6px; font-size: 12px; overflow-x: auto; max-height: 400px;">{{ consoleResponse() | json }}</pre>
          </div>
        </div>
      </section>
    </div>
  `
})
export class App {
  private http = inject(HttpClient);

  apiUrl = signal(typeof window !== 'undefined' && window.location.port === '4200' ? '' : 'http://localhost:5214');
  token = signal<string>(typeof localStorage !== 'undefined' ? localStorage.getItem('token') || '' : '');
  currentUser = signal<any>(null);
  userRole = signal<string>('Guest');

  authLoading = signal(false);
  authError = signal('');
  loginEmail = '';
  loginPassword = '';

  activeTab = signal<'dashboard' | 'colleges' | 'companies' | 'jobs' | 'applications' | 'interviews' | 'api-console'>('dashboard');

  // Tab Data
  platformReport = signal<any>(null);
  studentReport = signal<any>(null);
  companyReport = signal<any>(null);

  colleges = signal<any[]>([]);
  showAddCollegeModal = signal(false);
  newCollegeName = '';
  newCollegeCity = '';
  newCollegeAddress = '';
  newCollegePhone = '';
  newCollegeEmail = '';
  collegeSaving = signal(false);
  collegeError = signal('');
  companies = signal<any[]>([]);
  jobs = signal<any[]>([]);
  selectedJobRounds = signal<any[] | null>(null);
  currentJobId = signal<number>(0);

  applications = signal<any[]>([]);
  filterJobId = '1';
  currentHistory = signal<any[] | null>(null);
  selectedAppId = signal<number>(0);

  schedules = signal<any[]>([]);

  // Console State
  consoleMethod = 'GET';
  consoleEndpoint = '/api/reports/platform-overview';
  consoleBody = '{}';
  consoleLoading = signal(false);
  consoleStatus = signal<number>(0);
  consoleResponse = signal<any>(null);

  constructor() {
    if (typeof localStorage !== 'undefined') {
      const savedToken = localStorage.getItem('token');
      const savedUser = localStorage.getItem('currentUser');
      if (savedToken && savedUser) {
        this.token.set(savedToken);
        const user = JSON.parse(savedUser);
        this.currentUser.set(user);
        this.userRole.set(user.roles?.[0] || 'User');
        this.loadReports();
      }
    }
  }

  setTab(tab: 'dashboard' | 'colleges' | 'companies' | 'jobs' | 'applications' | 'interviews' | 'api-console') {
    this.activeTab.set(tab);
    if (tab === 'dashboard') this.loadReports();
    if (tab === 'colleges') this.loadColleges();
    if (tab === 'companies') this.loadCompanies();
    if (tab === 'jobs') this.loadJobs();
    if (tab === 'applications') this.loadApplications();
    if (tab === 'interviews') this.loadInterviews();
  }

  getHeaders() {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (this.token()) {
      headers = headers.set('Authorization', `Bearer ${this.token()}`);
    }
    return headers;
  }

  quickLogin(email: string, pass: string) {
    this.loginEmail = email;
    this.loginPassword = pass;
    this.login();
  }

  login() {
    this.authLoading.set(true);
    this.authError.set('');

    this.http.post<AuthResponse>(`${this.apiUrl()}/api/auth/login`, {
      email: this.loginEmail,
      password: this.loginPassword
    }).subscribe({
      next: (res) => {
        this.token.set(res.token);
        this.currentUser.set(res);
        this.userRole.set(res.roles[0] || 'User');
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem('token', res.token);
          localStorage.setItem('currentUser', JSON.stringify(res));
        }
        this.authLoading.set(false);
        this.loadReports();
      },
      error: (err) => {
        this.authLoading.set(false);
        this.authError.set(err.error?.message || 'Login failed. Please check credentials.');
      }
    });
  }

  logout() {
    this.token.set('');
    this.currentUser.set(null);
    this.userRole.set('Guest');
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('currentUser');
    }
    this.platformReport.set(null);
    this.studentReport.set(null);
    this.companyReport.set(null);
  }

  loadReports() {
    this.http.get(`${this.apiUrl()}/api/reports/platform-overview`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.platformReport.set(data), error: () => this.platformReport.set(null) });

    if (this.userRole() === 'Student') {
      this.http.get(`${this.apiUrl()}/api/reports/student-placement`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.studentReport.set(data), error: () => this.studentReport.set(null) });
    }

    if (this.userRole() === 'CompanyHR') {
      this.http.get(`${this.apiUrl()}/api/reports/company-recruitment`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.companyReport.set(data), error: () => this.companyReport.set(null) });
    }
  }

  loadColleges() {
    this.http.get<any[]>(`${this.apiUrl()}/api/colleges`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.colleges.set(data), error: () => this.colleges.set([]) });
  }

  createCollege() {
    if (!this.newCollegeName || !this.newCollegeCity) {
      this.collegeError.set('College Name and City are required.');
      return;
    }

    this.collegeSaving.set(true);
    this.collegeError.set('');

    this.http.post(`${this.apiUrl()}/api/colleges`, {
      collegeName: this.newCollegeName,
      city: this.newCollegeCity,
      address: this.newCollegeAddress,
      phoneNumber: this.newCollegePhone,
      email: this.newCollegeEmail
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.collegeSaving.set(false);
        this.showAddCollegeModal.set(false);
        this.newCollegeName = '';
        this.newCollegeCity = '';
        this.newCollegeAddress = '';
        this.newCollegePhone = '';
        this.newCollegeEmail = '';
        this.loadColleges();
      },
      error: (err) => {
        this.collegeSaving.set(false);
        this.collegeError.set(err.error?.message || 'Failed to create college.');
      }
    });
  }

  deleteCollege(collegeId: number) {
    if (!confirm(`Are you sure you want to delete College #${collegeId}?`)) return;
    this.http.delete(`${this.apiUrl()}/api/colleges/${collegeId}`, { headers: this.getHeaders() })
      .subscribe({
        next: () => this.loadColleges(),
        error: (err) => alert(err.error?.message || 'Failed to delete college.')
      });
  }

  loadCompanies() {
    this.http.get<any[]>(`${this.apiUrl()}/api/companies`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.companies.set(data), error: () => this.companies.set([]) });
  }

  loadJobs() {
    this.http.get<any[]>(`${this.apiUrl()}/api/jobs`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.jobs.set(data), error: () => this.jobs.set([]) });
  }

  viewRounds(jobId: number) {
    this.currentJobId.set(jobId);
    this.http.get<any[]>(`${this.apiUrl()}/api/jobs/${jobId}/interview-rounds`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => this.selectedJobRounds.set(data),
        error: (err) => alert(err.error?.message || 'Access denied or no rounds found.')
      });
  }

  applyJob(jobId: number) {
    this.http.post(`${this.apiUrl()}/api/jobs/${jobId}/apply`, {}, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          alert('Successfully applied to job #' + jobId);
          this.setTab('applications');
        },
        error: (err) => alert(err.error?.message || 'Unable to apply.')
      });
  }

  loadApplications() {
    if (this.userRole() === 'Student') {
      this.http.get<any[]>(`${this.apiUrl()}/api/applications/my`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.applications.set(data), error: () => this.applications.set([]) });
    } else {
      const jobId = this.filterJobId || '1';
      this.http.get<any[]>(`${this.apiUrl()}/api/jobs/${jobId}/applications`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.applications.set(data), error: () => this.applications.set([]) });
    }
  }

  loadHistory(applicationId: number) {
    this.selectedAppId.set(applicationId);
    this.http.get<any[]>(`${this.apiUrl()}/api/applications/${applicationId}/history`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => this.currentHistory.set(data),
        error: (err) => alert(err.error?.message || 'History access denied.')
      });
  }

  shortlistApp(id: number) {
    this.http.put(`${this.apiUrl()}/api/applications/${id}/shortlist`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadApplications(), error: (err) => alert(err.error?.message || 'Failed') });
  }

  rejectApp(id: number) {
    this.http.put(`${this.apiUrl()}/api/applications/${id}/reject`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadApplications(), error: (err) => alert(err.error?.message || 'Failed') });
  }

  withdrawApp(id: number) {
    this.http.put(`${this.apiUrl()}/api/applications/${id}/withdraw`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadApplications(), error: (err) => alert(err.error?.message || 'Failed') });
  }

  loadInterviews() {
    if (this.userRole() === 'Student') {
      this.http.get<any[]>(`${this.apiUrl()}/api/interview-schedules/my-schedules`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.schedules.set(data), error: () => this.schedules.set([]) });
    } else {
      const appId = this.selectedAppId() || 5;
      this.http.get<any[]>(`${this.apiUrl()}/api/applications/${appId}/interviews`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.schedules.set(data), error: () => this.schedules.set([]) });
    }
  }

  executeConsoleRequest() {
    this.consoleLoading.set(true);
    this.consoleResponse.set(null);

    let url = this.consoleEndpoint.startsWith('http')
      ? this.consoleEndpoint
      : `${this.apiUrl()}${this.consoleEndpoint.startsWith('/') ? '' : '/'}${this.consoleEndpoint}`;

    let body: any = null;
    if (this.consoleMethod !== 'GET' && this.consoleBody) {
      try {
        body = JSON.parse(this.consoleBody);
      } catch (e) {
        alert('Invalid JSON in Request Payload');
        this.consoleLoading.set(false);
        return;
      }
    }

    this.http.request(this.consoleMethod, url, {
      body,
      headers: this.getHeaders(),
      observe: 'response'
    }).subscribe({
      next: (res) => {
        this.consoleStatus.set(res.status);
        this.consoleResponse.set(res.body);
        this.consoleLoading.set(false);
      },
      error: (err: any) => {
        this.consoleStatus.set(err.status || 0);
        this.consoleResponse.set(err.error || err.message);
        this.consoleLoading.set(false);
      }
    });
  }

  getStatusName(status: number): string {
    const map: Record<number, string> = {
      1: 'Applied',
      2: 'UnderReview',
      3: 'Shortlisted',
      4: 'Interview',
      5: 'Selected',
      6: 'Rejected',
      7: 'Withdrawn'
    };
    return map[status] || 'Unknown';
  }

  getStatusBadge(status: number): string {
    switch (status) {
      case 1: return 'badge-blue';
      case 2: return 'badge-yellow';
      case 3: return 'badge-purple';
      case 4: return 'badge-blue';
      case 5: return 'badge-green';
      case 6: return 'badge-red';
      case 7: return 'badge-gray';
      default: return 'badge-gray';
    }
  }
}
