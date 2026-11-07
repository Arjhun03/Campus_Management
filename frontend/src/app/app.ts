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
      <div style="width: 100%; max-width: 480px; background: white; border-radius: 12px; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px -5px rgba(0,0,0,0.05); padding: 32px;">
        <div style="text-align: center; margin-bottom: 24px;">
          <div style="font-size: 40px; line-height: 1; margin-bottom: 12px;">🎓</div>
          <h1 style="font-size: 22px; font-weight: 700; color: #0f172a;">Campus Placement System</h1>
          <p style="font-size: 13px; color: #64748b; margin-top: 4px;">Select any of the 4 roles below or enter your credentials</p>
        </div>

        <!-- 4-ROLE 1-CLICK TEST LOGIN BUTTONS -->
        <div style="margin-bottom: 22px; padding: 14px; background: #f1f5f9; border-radius: 8px; border: 1px solid #e2e8f0;">
          <div style="font-size: 11px; font-weight: 700; color: #475569; text-transform: uppercase; margin-bottom: 10px; text-align: center; letter-spacing: 0.5px;">
            ⚡ 1-Click Role Login (All 4 System Roles)
          </div>
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px;">
            <button type="button" class="btn-primary btn-sm" style="background: #7c3aed; border-color: #6d28d9;" (click)="quickLogin('admin@gmail.com', 'Admin@123')">
              👑 SuperAdmin
            </button>
            <button type="button" class="btn-primary btn-sm" style="background: #0284c7; border-color: #0369a1;" (click)="quickLogin('officer@nit.edu', 'Officer@123')">
              🎓 PlacementOfficer
            </button>
            <button type="button" class="btn-primary btn-sm" style="background: #2563eb; border-color: #1d4ed8;" (click)="quickLogin('sarah.hr@tcs.com', 'HrPassword@123')">
              🏢 CompanyHR
            </button>
            <button type="button" class="btn-primary btn-sm" style="background: #16a34a; border-color: #15803d;" (click)="quickLogin('arun.cse@nit.edu', 'Student@123')">
              🎒 Student
            </button>
          </div>
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

        <div style="margin-top: 24px; text-align: center; border-top: 1px solid #f1f5f9; padding-top: 16px;">
          <a href="/scalar/v1" target="_blank" style="font-size: 12px; color: #2563eb; font-weight: 500;">
            📚 Open Scalar OpenAPI Documentation ↗
          </a>
        </div>
      </div>
    </div>

    <!-- STATE 2: LOGGED IN -> ROLE-BASED TEST WORKSPACE -->
    <div *ngIf="token()" style="max-width: 1280px; margin: 0 auto; padding: 20px;">
      <!-- Top Navigation & Profile Bar -->
      <header style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #e2e8f0; padding-bottom: 16px; margin-bottom: 20px;">
        <div>
          <h1 style="font-size: 20px; font-weight: 700; color: #0f172a; display: flex; align-items: center; gap: 8px;">
            🎓 Campus Management — Role Workspace
          </h1>
          <p style="font-size: 12px; color: #64748b; margin-top: 2px;">
            Logged in as: <strong style="color: #0f172a;">{{ currentUser()?.fullName }}</strong> ({{ currentUser()?.email }}) ·
            Role: <span class="badge badge-purple">{{ userRole() }}</span>
          </p>
        </div>

        <div style="display: flex; align-items: center; gap: 10px;">
          <a href="/scalar/v1" target="_blank" class="btn-secondary btn-sm">📚 Scalar API Docs ↗</a>
          <button class="btn-danger btn-sm" (click)="logout()">Sign Out</button>
        </div>
      </header>

      <!-- Navigation Tabs -->
      <nav style="display: flex; gap: 6px; border-bottom: 1px solid #e2e8f0; margin-bottom: 20px; overflow-x: auto; padding-bottom: 4px;">
        <button [class]="activeTab() === 'dashboard' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('dashboard')">
          📊 Reports & Analytics
        </button>
        <button *ngIf="isSuperAdmin()" [class]="activeTab() === 'colleges' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('colleges')">
          🏛️ Colleges & Officers
        </button>
        <button *ngIf="isSuperAdmin() || userRole() === 'CompanyHR'" [class]="activeTab() === 'companies' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('companies')">
          🏢 Companies & HRs
        </button>
        <button *ngIf="isSuperAdmin() || userRole() === 'PlacementOfficer' || userRole() === 'Student'" [class]="activeTab() === 'students' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('students')">
          🧑‍🎓 Students
        </button>
        <button [class]="activeTab() === 'jobs' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('jobs')">
          💼 Jobs & Drives
        </button>
        <button [class]="activeTab() === 'applications' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('applications')">
          📋 Applications & History
        </button>
        <button [class]="activeTab() === 'interviews' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('interviews')">
          📅 Interviews
        </button>
        <button [class]="activeTab() === 'account' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('account')">
          🔐 Security
        </button>
        <button [class]="activeTab() === 'api-console' ? 'btn-primary' : 'btn-secondary'" (click)="setTab('api-console')">
          ⚡ API Console
        </button>
      </nav>

      <!-- TAB 1: REPORTS & ANALYTICS -->
      <section *ngIf="activeTab() === 'dashboard'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px;">
          <h2 style="font-size: 16px; font-weight: 600;">Reports & Intelligence (ReportsController)</h2>
          <button class="btn-secondary btn-sm" (click)="loadReports()">🔄 Refresh Reports</button>
        </div>

        <!-- SuperAdmin / Admin Platform Overview -->
        <div *ngIf="platformReport()" class="card">
          <h3 style="font-size: 13px; font-weight: 700; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            👑 Global Platform Overview (SuperAdmin)
          </h3>
          <div class="stat-grid">
            <div class="stat-box"><div class="stat-value">{{ platformReport().totalColleges }}</div><div class="stat-label">Colleges</div></div>
            <div class="stat-box"><div class="stat-value">{{ platformReport().totalCompanies }}</div><div class="stat-label">Companies</div></div>
            <div class="stat-box"><div class="stat-value">{{ platformReport().totalStudents }}</div><div class="stat-label">Students</div></div>
            <div class="stat-box"><div class="stat-value">{{ platformReport().totalJobs }}</div><div class="stat-label">Jobs Posted</div></div>
            <div class="stat-box"><div class="stat-value">{{ platformReport().totalApplications }}</div><div class="stat-label">Applications</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #16a34a;">{{ platformReport().overallPlacementRatePercentage }}%</div><div class="stat-label">Placement Rate</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #0284c7;">₹{{ platformReport().highestPackageOffered | number }}</div><div class="stat-label">Highest CTC</div></div>
          </div>
        </div>

        <!-- PlacementOfficer College Report -->
        <div *ngIf="collegeReport()" class="card" style="border-left: 4px solid #0284c7;">
          <h3 style="font-size: 13px; font-weight: 700; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            🎓 College Placement Overview ({{ collegeReport().collegeName }})
          </h3>
          <div class="stat-grid">
            <div class="stat-box"><div class="stat-value">{{ collegeReport().totalStudents }}</div><div class="stat-label">Enrolled Students</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #16a34a;">{{ collegeReport().placedStudents }}</div><div class="stat-label">Placed Students</div></div>
            <div class="stat-box"><div class="stat-value">{{ collegeReport().placementPercentage }}%</div><div class="stat-label">Placement Rate</div></div>
            <div class="stat-box"><div class="stat-value">₹{{ collegeReport().averagePackage | number }}</div><div class="stat-label">Average CTC</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #0284c7;">₹{{ collegeReport().highestPackage | number }}</div><div class="stat-label">Highest CTC</div></div>
          </div>
        </div>

        <!-- CompanyHR Report -->
        <div *ngIf="companyReport()" class="card" style="border-left: 4px solid #2563eb;">
          <h3 style="font-size: 13px; font-weight: 700; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            🏢 Company Recruitment Overview ({{ companyReport().companyName }})
          </h3>
          <div class="stat-grid">
            <div class="stat-box"><div class="stat-value">{{ companyReport().totalJobsPosted }}</div><div class="stat-label">Jobs Posted</div></div>
            <div class="stat-box"><div class="stat-value">{{ companyReport().totalApplicants }}</div><div class="stat-label">Total Applicants</div></div>
            <div class="stat-box"><div class="stat-value">{{ companyReport().interviewsConducted }}</div><div class="stat-label">Interviews Done</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #16a34a;">{{ companyReport().candidatesSelected }}</div><div class="stat-label">Candidates Selected</div></div>
          </div>
        </div>

        <!-- Student Placement Summary -->
        <div *ngIf="studentReport()" class="card" style="border-left: 4px solid #16a34a;">
          <h3 style="font-size: 13px; font-weight: 700; color: #475569; margin-bottom: 12px; text-transform: uppercase;">
            🎒 My Placement Summary ({{ studentReport().studentName }})
          </h3>
          <div class="stat-grid">
            <div class="stat-box"><div class="stat-value">{{ studentReport().totalApplications }}</div><div class="stat-label">Applied</div></div>
            <div class="stat-box"><div class="stat-value">{{ studentReport().interviewCount }}</div><div class="stat-label">Interviews</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #16a34a;">{{ studentReport().selectedCount }}</div><div class="stat-label">Selected Offers</div></div>
            <div class="stat-box"><div class="stat-value" style="color: #dc2626;">{{ studentReport().rejectedCount }}</div><div class="stat-label">Rejected</div></div>
          </div>
        </div>
      </section>

      <!-- TAB 2: COLLEGES & PLACEMENT OFFICERS -->
      <section *ngIf="activeTab() === 'colleges'">
        <!-- Colleges Section -->
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Registered Colleges (CollegesController)</h2>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="showAddCollegeModal.set(!showAddCollegeModal())">
              ➕ Add College
            </button>
            <button class="btn-secondary btn-sm" (click)="loadColleges()">🔄 Refresh</button>
          </div>
        </div>

        <!-- Add College Form -->
        <div *ngIf="showAddCollegeModal()" class="card" style="border-left: 4px solid #7c3aed; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Register New College (POST /api/colleges)</h3>
          <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">College Name *</label>
              <input [(ngModel)]="newCollegeName" placeholder="e.g. IIT Madras" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">City *</label>
              <input [(ngModel)]="newCollegeCity" placeholder="e.g. Chennai" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Email</label>
              <input [(ngModel)]="newCollegeEmail" placeholder="info@college.edu" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Phone</label>
              <input [(ngModel)]="newCollegePhone" placeholder="+91 9876543210" style="width: 100%;" />
            </div>
          </div>
          <div style="margin-bottom: 12px;">
            <label style="font-size: 11px; font-weight: 600;">Campus Address</label>
            <input [(ngModel)]="newCollegeAddress" placeholder="Full address" style="width: 100%;" />
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="createCollege()" [disabled]="collegeSaving()">Save College</button>
            <button class="btn-secondary btn-sm" (click)="showAddCollegeModal.set(false)">Cancel</button>
            <span *ngIf="collegeError()" style="color: #dc2626; font-size: 12px;">{{ collegeError() }}</span>
          </div>
        </div>

        <!-- Colleges Table -->
        <div class="card table-responsive" style="margin-bottom: 24px;">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>College Name</th>
                <th>City</th>
                <th>Email</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of colleges()">
                <td>#{{ c.collegeId }}</td>
                <td style="font-weight: 600;">{{ c.collegeName }}</td>
                <td>{{ c.city }}</td>
                <td>{{ c.email || '—' }}</td>
                <td><span class="badge badge-green">Active</span></td>
                <td>
                  <button class="btn-danger btn-sm" (click)="deleteCollege(c.collegeId)">Delete</button>
                </td>
              </tr>
              <tr *ngIf="colleges().length === 0"><td colspan="6" style="text-align: center; color: #64748b; padding: 20px;">No colleges found.</td></tr>
            </tbody>
          </table>
        </div>

        <!-- Placement Officers Section -->
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Placement Officers (PlacementOfficersController)</h2>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="showAddOfficerModal.set(!showAddOfficerModal())">
              ➕ Add Placement Officer
            </button>
            <button class="btn-secondary btn-sm" (click)="loadOfficers()">🔄 Refresh</button>
          </div>
        </div>

        <!-- Add Officer Form -->
        <div *ngIf="showAddOfficerModal()" class="card" style="border-left: 4px solid #0284c7; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Create Placement Officer (POST /api/placementofficers)</h3>
          <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Full Name *</label>
              <input [(ngModel)]="newOfficerName" placeholder="Dr. Jane Doe" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Email *</label>
              <input [(ngModel)]="newOfficerEmail" placeholder="officer@college.edu" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Phone Number</label>
              <input [(ngModel)]="newOfficerPhone" placeholder="+91 9444000000" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Assigned College ID *</label>
              <input [(ngModel)]="newOfficerCollegeId" type="number" placeholder="1" style="width: 100%;" />
            </div>
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="createOfficer()">Save Officer</button>
            <button class="btn-secondary btn-sm" (click)="showAddOfficerModal.set(false)">Cancel</button>
          </div>
        </div>

        <!-- Placement Officers Table -->
        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>College</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let o of placementOfficers()">
                <td>#{{ o.placementOfficerId }}</td>
                <td style="font-weight: 600;">{{ o.fullName }}</td>
                <td>{{ o.email }}</td>
                <td>{{ o.phoneNumber || '—' }}</td>
                <td><span class="badge badge-blue">{{ o.collegeName || '#' + o.collegeId }}</span></td>
                <td>
                  <button class="btn-danger btn-sm" (click)="deleteOfficer(o.placementOfficerId)">Delete</button>
                </td>
              </tr>
              <tr *ngIf="placementOfficers().length === 0"><td colspan="6" style="text-align: center; color: #64748b; padding: 20px;">No officers loaded. Click Refresh to load.</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 3: COMPANIES & HR APPROVALS -->
      <section *ngIf="activeTab() === 'companies'">
        <!-- Companies Section -->
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Registered Companies (CompaniesController)</h2>
          <div style="display: flex; gap: 8px;">
            <button *ngIf="isSuperAdmin()" class="btn-primary btn-sm" (click)="showAddCompanyModal.set(!showAddCompanyModal())">
              ➕ Add Company
            </button>
            <button class="btn-secondary btn-sm" (click)="loadCompanies()">🔄 Refresh</button>
          </div>
        </div>

        <!-- Add Company Form -->
        <div *ngIf="showAddCompanyModal()" class="card" style="border-left: 4px solid #2563eb; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Register Company (POST /api/companies)</h3>
          <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Company Name *</label>
              <input [(ngModel)]="newCompanyName" placeholder="e.g. Infosys Technologies" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Company Email *</label>
              <input [(ngModel)]="newCompanyEmail" placeholder="hr@infosys.com" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Industry *</label>
              <input [(ngModel)]="newCompanyIndustry" placeholder="Information Technology" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Website URL</label>
              <input [(ngModel)]="newCompanyWebsite" placeholder="https://infosys.com" style="width: 100%;" />
            </div>
          </div>
          <div style="margin-bottom: 12px;">
            <label style="font-size: 11px; font-weight: 600;">Address</label>
            <input [(ngModel)]="newCompanyAddress" placeholder="Headquarters address" style="width: 100%;" />
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="createCompany()">Save Company</button>
            <button class="btn-secondary btn-sm" (click)="showAddCompanyModal.set(false)">Cancel</button>
          </div>
        </div>

        <div class="card table-responsive" style="margin-bottom: 24px;">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Company Name</th>
                <th>Industry</th>
                <th>Website</th>
                <th>Email</th>
                <th *ngIf="isSuperAdmin()">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of companies()">
                <td>#{{ c.companyId }}</td>
                <td style="font-weight: 600;">{{ c.companyName }}</td>
                <td>{{ c.industry }}</td>
                <td><a [href]="c.websiteUrl || c.website" target="_blank">{{ c.websiteUrl || c.website || '—' }}</a></td>
                <td>{{ c.companyEmail || '—' }}</td>
                <td *ngIf="isSuperAdmin()">
                  <button class="btn-danger btn-sm" (click)="deleteCompany(c.companyId)">Delete</button>
                </td>
              </tr>
              <tr *ngIf="companies().length === 0"><td [attr.colspan]="isSuperAdmin() ? 6 : 5" style="text-align: center; color: #64748b; padding: 20px;">No companies found.</td></tr>
            </tbody>
          </table>
        </div>

        <!-- Company HR Approvals (SuperAdmin only) -->
        <div *ngIf="isSuperAdmin()">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
            <h2 style="font-size: 16px; font-weight: 600;">Company HR Approvals (CompanyHRsController)</h2>
            <button class="btn-secondary btn-sm" (click)="loadPendingHRs()">🔄 Load Pending HRs</button>
          </div>

          <div class="card table-responsive">
            <table>
              <thead>
                <tr>
                  <th>HR ID</th>
                  <th>Full Name</th>
                  <th>Email</th>
                  <th>Company</th>
                  <th>Designation</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let h of pendingHRs()">
                  <td>#{{ h.companyHRId }}</td>
                  <td style="font-weight: 600;">{{ h.fullName }}</td>
                  <td>{{ h.email }}</td>
                  <td>{{ h.companyName }}</td>
                  <td>{{ h.designation }}</td>
                  <td><span class="badge badge-yellow">Pending</span></td>
                  <td style="display: flex; gap: 6px;">
                    <button class="btn-success btn-sm" (click)="approveHR(h.companyHRId)">Approve</button>
                    <button class="btn-danger btn-sm" (click)="rejectHR(h.companyHRId)">Reject</button>
                  </td>
                </tr>
                <tr *ngIf="pendingHRs().length === 0"><td colspan="7" style="text-align: center; color: #64748b; padding: 20px;">No pending HR registrations.</td></tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- My Company HR Profile (CompanyHR role) -->
        <div *ngIf="userRole() === 'CompanyHR' && myHRProfile()" class="card" style="border-left: 4px solid #2563eb;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 8px;">My HR Profile (GET /api/company-hrs/me)</h3>
          <p><strong>Name:</strong> {{ myHRProfile().fullName }}</p>
          <p><strong>Designation:</strong> {{ myHRProfile().designation }}</p>
          <p><strong>Company:</strong> {{ myHRProfile().companyName }}</p>
          <p><strong>Approved:</strong> <span class="badge badge-green">{{ myHRProfile().status === 1 ? 'Approved' : 'Pending' }}</span></p>
        </div>
      </section>

      <!-- TAB 4: STUDENTS MANAGEMENT -->
      <section *ngIf="activeTab() === 'students'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Student Management (StudentsController)</h2>
          <div style="display: flex; gap: 8px;">
            <button *ngIf="isSuperAdmin() || userRole() === 'PlacementOfficer'" class="btn-primary btn-sm" (click)="showEnrollModal.set(!showEnrollModal())">
              ➕ Enroll Single Student
            </button>
            <button *ngIf="isSuperAdmin() || userRole() === 'PlacementOfficer'" class="btn-secondary btn-sm" (click)="loadPendingStudents()">
              ⏳ Pending Approvals
            </button>
            <button class="btn-secondary btn-sm" (click)="loadStudents()">🔄 Refresh Students</button>
          </div>
        </div>

        <!-- Student Self Profile (Student role) -->
        <div *ngIf="userRole() === 'Student' && myStudentProfile()" class="card" style="border-left: 4px solid #16a34a; margin-bottom: 20px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 10px;">🎓 My Student Profile (GET /api/students/me)</h3>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px;">
            <div><strong>Reg Number:</strong> {{ myStudentProfile().registerNumber }}</div>
            <div><strong>Name:</strong> {{ myStudentProfile().fullName }}</div>
            <div><strong>College:</strong> {{ myStudentProfile().collegeName }}</div>
            <div><strong>Department:</strong> {{ myStudentProfile().department }}</div>
            <div><strong>CGPA:</strong> <span class="badge badge-green">{{ myStudentProfile().cgpa }}</span></div>
            <div><strong>Arrears:</strong> {{ myStudentProfile().activeBacklogs }}</div>
          </div>
        </div>

        <!-- Enroll Student Form -->
        <div *ngIf="showEnrollModal()" class="card" style="border-left: 4px solid #0284c7; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Enroll New Student (POST /api/students/enroll)</h3>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Full Name *</label>
              <input [(ngModel)]="newStudentName" placeholder="Candidate Name" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Email *</label>
              <input [(ngModel)]="newStudentEmail" placeholder="student@college.edu" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Initial Password *</label>
              <input [(ngModel)]="newStudentPassword" placeholder="Student@123" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Register Number *</label>
              <input [(ngModel)]="newStudentRegNo" placeholder="NIT2026CSE099" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Department *</label>
              <input [(ngModel)]="newStudentDept" placeholder="Computer Science" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Degree *</label>
              <input [(ngModel)]="newStudentDegree" placeholder="B.Tech" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Graduation Year *</label>
              <input [(ngModel)]="newStudentGradYear" type="number" placeholder="2026" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">CGPA *</label>
              <input [(ngModel)]="newStudentCGPA" type="number" step="0.01" placeholder="8.50" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Active Arrears</label>
              <input [(ngModel)]="newStudentBacklogs" type="number" placeholder="0" style="width: 100%;" />
            </div>
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="enrollStudent()">Enroll Student</button>
            <button class="btn-secondary btn-sm" (click)="showEnrollModal.set(false)">Cancel</button>
          </div>
        </div>

        <!-- Students Table -->
        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Reg No</th>
                <th>Name</th>
                <th>Department</th>
                <th>CGPA</th>
                <th>Arrears</th>
                <th>Status</th>
                <th *ngIf="isSuperAdmin() || userRole() === 'PlacementOfficer'">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let s of students()">
                <td>#{{ s.studentId }}</td>
                <td style="font-weight: 600;">{{ s.registerNumber }}</td>
                <td>{{ s.fullName }}</td>
                <td>{{ s.department }} ({{ s.graduationYear }})</td>
                <td><span class="badge badge-green">{{ s.cgpa }}</span></td>
                <td>{{ s.activeBacklogs }}</td>
                <td>
                  <span class="badge" [ngClass]="s.status === 1 ? 'badge-green' : s.status === 2 ? 'badge-red' : 'badge-yellow'">
                    {{ s.status === 1 ? 'Approved' : s.status === 2 ? 'Rejected' : s.status === 3 ? 'Suspended' : 'Pending' }}
                  </span>
                </td>
                <td *ngIf="isSuperAdmin() || userRole() === 'PlacementOfficer'" style="display: flex; gap: 4px;">
                  <button *ngIf="s.status !== 1" class="btn-success btn-sm" (click)="approveStudent(s.studentId)">Approve</button>
                  <button *ngIf="s.status !== 2" class="btn-danger btn-sm" (click)="rejectStudent(s.studentId)">Reject</button>
                  <button *ngIf="s.status === 1" class="btn-secondary btn-sm" (click)="suspendStudent(s.studentId)">Suspend</button>
                </td>
              </tr>
              <tr *ngIf="students().length === 0"><td colspan="8" style="text-align: center; color: #64748b; padding: 20px;">No students found.</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 5: JOBS & DRIVES -->
      <section *ngIf="activeTab() === 'jobs'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Job Openings & Recruitment Drives (JobsController)</h2>
          <div style="display: flex; gap: 8px;">
            <button *ngIf="userRole() === 'CompanyHR'" class="btn-primary btn-sm" (click)="showCreateJobModal.set(!showCreateJobModal())">
              ➕ Post New Job
            </button>
            <button *ngIf="userRole() === 'Student'" class="btn-secondary btn-sm" (click)="loadEligibleJobs()">
              🎯 Show Eligible Only
            </button>
            <button class="btn-secondary btn-sm" (click)="loadJobs()">🔄 Refresh All</button>
          </div>
        </div>

        <!-- Post New Job Modal (CompanyHR) -->
        <div *ngIf="showCreateJobModal()" class="card" style="border-left: 4px solid #2563eb; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Create Job Opening (POST /api/jobs)</h3>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Job Title *</label>
              <input [(ngModel)]="newJobTitle" placeholder="e.g. Software Engineer" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Job Location *</label>
              <input [(ngModel)]="newJobLocation" placeholder="e.g. Bangalore / Chennai" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Package (CTC in LPA)</label>
              <input [(ngModel)]="newJobPackage" type="number" placeholder="850000" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Min CGPA</label>
              <input [(ngModel)]="newJobMinCGPA" type="number" step="0.1" placeholder="7.0" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Max Arrears Allowed</label>
              <input [(ngModel)]="newJobMaxArrears" type="number" placeholder="0" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Open to All Colleges?</label>
              <select [(ngModel)]="newJobOpenToAll" style="width: 100%;">
                <option [ngValue]="true">Yes (Open to all)</option>
                <option [ngValue]="false">No (Restricted to Target Colleges)</option>
              </select>
            </div>
          </div>
          <div style="margin-bottom: 12px;">
            <label style="font-size: 11px; font-weight: 600;">Job Description</label>
            <input [(ngModel)]="newJobDesc" placeholder="Responsibilities and role summary" style="width: 100%;" />
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="createJob()">Post Job Opening</button>
            <button class="btn-secondary btn-sm" (click)="showCreateJobModal.set(false)">Cancel</button>
          </div>
        </div>

        <!-- Jobs Table -->
        <div class="card table-responsive" style="margin-bottom: 20px;">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Title</th>
                <th>Company</th>
                <th>Location</th>
                <th>Package</th>
                <th>Min CGPA</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let j of jobs()">
                <td>#{{ j.jobId }}</td>
                <td style="font-weight: 600;">{{ j.title }}</td>
                <td>{{ j.companyName }}</td>
                <td>{{ j.location }}</td>
                <td>₹{{ j.salaryPackage | number }}</td>
                <td>{{ j.minCgpa || 0 }}</td>
                <td>
                  <span class="badge" [ngClass]="j.status === 1 ? 'badge-green' : j.status === 2 ? 'badge-red' : 'badge-yellow'">
                    {{ j.statusText || (j.status === 1 ? 'Published' : j.status === 2 ? 'Closed' : 'Draft') }}
                  </span>
                </td>
                <td style="display: flex; gap: 4px; flex-wrap: wrap;">
                  <button class="btn-secondary btn-sm" (click)="viewRounds(j.jobId)">Rounds</button>
                  <button class="btn-secondary btn-sm" (click)="viewDrives(j.jobId)">Drives</button>
                  <ng-container *ngIf="userRole() === 'CompanyHR'">
                    <button *ngIf="j.status === 0" class="btn-success btn-sm" (click)="publishJob(j.jobId)">Publish</button>
                    <button *ngIf="j.status === 1" class="btn-danger btn-sm" (click)="closeJob(j.jobId)">Close</button>
                    <button *ngIf="j.status === 0" class="btn-danger btn-sm" (click)="deleteJob(j.jobId)">Delete</button>
                    <button class="btn-secondary btn-sm" (click)="openAddRound(j.jobId)">+ Round</button>
                    <button class="btn-secondary btn-sm" (click)="openAddDrive(j.jobId)">+ Drive</button>
                  </ng-container>
                  <ng-container *ngIf="userRole() === 'Student'">
                    <button class="btn-secondary btn-sm" (click)="checkEligibility(j.jobId)">Check Eligible</button>
                    <button class="btn-primary btn-sm" (click)="applyJob(j.jobId)">Apply</button>
                  </ng-container>
                </td>
              </tr>
              <tr *ngIf="jobs().length === 0"><td colspan="8" style="text-align: center; color: #64748b; padding: 20px;">No job openings found.</td></tr>
            </tbody>
          </table>
        </div>

        <!-- Rounds Drawer / Panel -->
        <div *ngIf="selectedJobRounds() !== null" class="card" style="border-left: 4px solid #2563eb; margin-bottom: 16px;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
            <h3 style="font-size: 14px; font-weight: 600;">
              Interview Rounds for Job #{{ currentJobId() }} ({{ selectedJobRounds()?.length }} Rounds)
            </h3>
            <button class="btn-secondary btn-sm" (click)="selectedJobRounds.set(null)">✕ Close</button>
          </div>
          <div *ngIf="selectedJobRounds()?.length === 0" style="color: #64748b; font-size: 13px;">No rounds configured.</div>
          <div *ngFor="let r of selectedJobRounds()" style="padding: 10px 14px; background: #f8fafc; border-radius: 6px; margin-bottom: 8px; display: flex; justify-content: space-between; align-items: center;">
            <div>
              <strong>Round {{ r.roundNumber }}: {{ r.roundName }}</strong>
              <span class="badge badge-blue" style="margin-left: 8px;">{{ r.roundType }}</span>
              <p style="font-size: 12px; color: #64748b; margin-top: 2px;">{{ r.description }}</p>
            </div>
            <button *ngIf="userRole() === 'CompanyHR'" class="btn-danger btn-sm" (click)="deleteRound(r.roundId)">Delete</button>
          </div>
        </div>

        <!-- Drives Panel -->
        <div *ngIf="selectedJobDrives() !== null" class="card" style="border-left: 4px solid #7c3aed;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
            <h3 style="font-size: 14px; font-weight: 600;">
              Recruitment Drives for Job #{{ currentJobId() }}
            </h3>
            <button class="btn-secondary btn-sm" (click)="selectedJobDrives.set(null)">✕ Close</button>
          </div>
          <div *ngIf="selectedJobDrives()?.length === 0" style="color: #64748b; font-size: 13px;">No recruitment drives scheduled.</div>
          <div *ngFor="let d of selectedJobDrives()" style="padding: 10px 14px; background: #f8fafc; border-radius: 6px; margin-bottom: 8px; display: flex; justify-content: space-between; align-items: center;">
            <div>
              <strong>{{ d.driveName }}</strong> — {{ d.driveDate | date:'mediumDate' }}
              <span class="badge badge-purple" style="margin-left: 8px;">{{ d.statusText }}</span>
              <p style="font-size: 12px; color: #64748b; margin-top: 2px;">Venue: {{ d.venue || 'Virtual' }}</p>
            </div>
            <div *ngIf="userRole() === 'CompanyHR'" style="display: flex; gap: 4px;">
              <button class="btn-danger btn-sm" (click)="cancelDrive(d.driveId)">Cancel</button>
              <button class="btn-danger btn-sm" (click)="deleteDrive(d.driveId)">Delete</button>
            </div>
          </div>
        </div>
      </section>

      <!-- TAB 6: APPLICATIONS & HISTORY -->
      <section *ngIf="activeTab() === 'applications'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Applications (JobApplicationsController)</h2>
          <div style="display: flex; gap: 8px;">
            <input *ngIf="userRole() !== 'Student'" [(ngModel)]="filterJobId" placeholder="Filter Job ID" style="width: 140px;" />
            <button class="btn-secondary btn-sm" (click)="loadApplications()">🔄 Refresh</button>
          </div>
        </div>

        <div class="card table-responsive" style="margin-bottom: 20px;">
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
                <td style="display: flex; gap: 4px; flex-wrap: wrap;">
                  <button class="btn-secondary btn-sm" (click)="loadHistory(a.applicationId)">History</button>
                  <ng-container *ngIf="userRole() === 'CompanyHR' || isSuperAdmin()">
                    <button class="btn-success btn-sm" (click)="shortlistApp(a.applicationId)">Shortlist</button>
                    <button class="btn-danger btn-sm" (click)="rejectApp(a.applicationId)">Reject</button>
                    <button class="btn-primary btn-sm" (click)="openScheduleModal(a.applicationId, a.jobId)">Schedule Interview</button>
                  </ng-container>
                  <ng-container *ngIf="userRole() === 'Student'">
                    <button class="btn-danger btn-sm" (click)="withdrawApp(a.applicationId)" [disabled]="a.status >= 5">Withdraw</button>
                  </ng-container>
                </td>
              </tr>
              <tr *ngIf="applications().length === 0"><td colspan="6" style="text-align: center; color: #64748b; padding: 20px;">No applications found.</td></tr>
            </tbody>
          </table>
        </div>

        <!-- History Timeline Modal -->
        <div *ngIf="currentHistory() !== null" class="card" style="border-left: 4px solid #16a34a;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
            <h3 style="font-size: 14px; font-weight: 600;">Application #{{ selectedAppId() }} Status History (ApplicationStatusHistoriesController)</h3>
            <button class="btn-secondary btn-sm" (click)="currentHistory.set(null)">✕ Close</button>
          </div>
          <div *ngIf="currentHistory()?.length === 0" style="color: #64748b;">No history records found.</div>
          <div style="border-left: 2px solid #e2e8f0; margin-left: 10px; padding-left: 16px;">
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

      <!-- TAB 7: INTERVIEW SCHEDULES -->
      <section *ngIf="activeTab() === 'interviews'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
          <h2 style="font-size: 16px; font-weight: 600;">Interview Schedules (InterviewSchedulesController)</h2>
          <button class="btn-secondary btn-sm" (click)="loadInterviews()">🔄 Refresh Schedules</button>
        </div>

        <!-- Schedule Interview Form Modal (CompanyHR) -->
        <div *ngIf="showScheduleModal()" class="card" style="border-left: 4px solid #2563eb; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Schedule Candidate Interview (POST /api/interview-schedules)</h3>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Application ID *</label>
              <input [(ngModel)]="newScheduleAppId" type="number" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Interview Round ID *</label>
              <input [(ngModel)]="newScheduleRoundId" type="number" placeholder="1" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Interviewer Name</label>
              <input [(ngModel)]="newScheduleInterviewer" placeholder="Interviewer Name" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Start Time *</label>
              <input [(ngModel)]="newScheduleStartTime" type="datetime-local" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">End Time *</label>
              <input [(ngModel)]="newScheduleEndTime" type="datetime-local" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Meeting Link / Venue</label>
              <input [(ngModel)]="newScheduleMeetingLink" placeholder="https://meet.google.com/xyz" style="width: 100%;" />
            </div>
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="submitSchedule()">Create Schedule</button>
            <button class="btn-secondary btn-sm" (click)="showScheduleModal.set(false)">Cancel</button>
          </div>
        </div>

        <!-- Feedback Modal (CompanyHR) -->
        <div *ngIf="showFeedbackModal()" class="card" style="border-left: 4px solid #16a34a; margin-bottom: 16px;">
          <h3 style="font-size: 14px; font-weight: 600; margin-bottom: 12px;">Submit Interview Feedback (PUT /api/interviews/&#123;id&#125;/feedback)</h3>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 12px;">
            <div>
              <label style="font-size: 11px; font-weight: 600;">Result *</label>
              <select [(ngModel)]="feedbackResult" style="width: 100%;">
                <option [ngValue]="1">Passed</option>
                <option [ngValue]="2">Failed</option>
                <option [ngValue]="0">Pending</option>
              </select>
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Rating (1 - 10)</label>
              <input [(ngModel)]="feedbackRating" type="number" min="1" max="10" placeholder="8" style="width: 100%;" />
            </div>
            <div>
              <label style="font-size: 11px; font-weight: 600;">Feedback Remarks</label>
              <input [(ngModel)]="feedbackRemarks" placeholder="Strong problem solving skills" style="width: 100%;" />
            </div>
          </div>
          <div style="display: flex; gap: 8px;">
            <button class="btn-primary btn-sm" (click)="submitFeedback()">Submit Feedback</button>
            <button class="btn-secondary btn-sm" (click)="showFeedbackModal.set(false)">Cancel</button>
          </div>
        </div>

        <div class="card table-responsive">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Round</th>
                <th>Candidate</th>
                <th>Start Time</th>
                <th>End Time</th>
                <th>Status</th>
                <th>Result</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let s of schedules()">
                <td>#{{ s.scheduleId }}</td>
                <td style="font-weight: 600;">{{ s.roundName || 'Round #' + s.roundId }}</td>
                <td>{{ s.studentName }}</td>
                <td>{{ s.startTime | date:'short' }}</td>
                <td>{{ s.endTime | date:'shortTime' }}</td>
                <td><span class="badge badge-blue">{{ s.statusText }}</span></td>
                <td><span class="badge badge-green">{{ s.resultText || 'Pending' }}</span></td>
                <td style="display: flex; gap: 4px;">
                  <button *ngIf="userRole() === 'CompanyHR'" class="btn-success btn-sm" (click)="openFeedback(s.scheduleId)">Feedback</button>
                  <button *ngIf="userRole() === 'CompanyHR'" class="btn-danger btn-sm" (click)="deleteSchedule(s.scheduleId)">Cancel</button>
                </td>
              </tr>
              <tr *ngIf="schedules().length === 0"><td colspan="8" style="text-align: center; color: #64748b; padding: 20px;">No interview schedules found.</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- TAB 8: SECURITY & PASSWORD -->
      <section *ngIf="activeTab() === 'account'">
        <div style="max-width: 500px;" class="card">
          <h2 style="font-size: 16px; font-weight: 600; margin-bottom: 14px;">Change Password (POST /api/auth/change-password)</h2>
          <div style="margin-bottom: 12px;">
            <label style="font-size: 11px; font-weight: 600; display: block; margin-bottom: 4px;">Current Password *</label>
            <input [(ngModel)]="currentPassword" type="password" placeholder="••••••••" style="width: 100%;" />
          </div>
          <div style="margin-bottom: 14px;">
            <label style="font-size: 11px; font-weight: 600; display: block; margin-bottom: 4px;">New Password (Min 8 chars, 1 uppercase, 1 special) *</label>
            <input [(ngModel)]="newPassword" type="password" placeholder="NewPassword@123" style="width: 100%;" />
          </div>
          <button class="btn-primary" (click)="changePassword()">Change Password</button>
        </div>
      </section>

      <!-- TAB 9: INTERACTIVE API CONSOLE -->
      <section *ngIf="activeTab() === 'api-console'">
        <div class="card">
          <h2 style="font-size: 16px; font-weight: 600; margin-bottom: 14px;">Interactive API Console (Any Controller Endpoint)</h2>
          <div style="display: flex; gap: 8px; margin-bottom: 14px;">
            <select [(ngModel)]="consoleMethod" style="width: 110px; font-weight: 600;">
              <option value="GET">GET</option>
              <option value="POST">POST</option>
              <option value="PUT">PUT</option>
              <option value="DELETE">DELETE</option>
            </select>
            <input [(ngModel)]="consoleEndpoint" placeholder="/api/colleges" style="flex: 1;" />
            <button class="btn-primary" (click)="runConsole()" [disabled]="consoleLoading()">
              {{ consoleLoading() ? 'Sending...' : '⚡ Send Request' }}
            </button>
          </div>
          <div *ngIf="consoleMethod !== 'GET'" style="margin-bottom: 14px;">
            <label style="font-size: 11px; font-weight: 600; color: #475569; display: block; margin-bottom: 4px;">Request Payload (JSON)</label>
            <textarea [(ngModel)]="consoleBody" rows="4" style="width: 100%; font-family: monospace; font-size: 12px;"></textarea>
          </div>
          <div *ngIf="consoleResponse() !== null" style="background: #0f172a; color: #f8fafc; padding: 14px; border-radius: 6px; font-family: monospace; font-size: 12px; max-height: 400px; overflow-y: auto;">
            <div style="margin-bottom: 8px; color: #38bdf8;">Status: {{ consoleStatus() }}</div>
            <pre>{{ consoleResponse() | json }}</pre>
          </div>
        </div>
      </section>
    </div>
  `
})
export class App {
  private http = inject(HttpClient);

  apiUrl = signal<string>(typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5214');
  token = signal<string>(typeof localStorage !== 'undefined' ? localStorage.getItem('token') || '' : '');
  currentUser = signal<any>(null);
  userRole = signal<string>('Guest');

  authLoading = signal(false);
  authError = signal('');
  loginEmail = '';
  loginPassword = '';

  activeTab = signal<'dashboard' | 'colleges' | 'companies' | 'students' | 'jobs' | 'applications' | 'interviews' | 'account' | 'api-console'>('dashboard');

  // Reports Data
  platformReport = signal<any>(null);
  collegeReport = signal<any>(null);
  companyReport = signal<any>(null);
  studentReport = signal<any>(null);

  // Colleges & Officers
  colleges = signal<any[]>([]);
  placementOfficers = signal<any[]>([]);
  showAddCollegeModal = signal(false);
  newCollegeName = '';
  newCollegeCity = '';
  newCollegeEmail = '';
  newCollegePhone = '';
  newCollegeAddress = '';
  collegeSaving = signal(false);
  collegeError = signal('');

  showAddOfficerModal = signal(false);
  newOfficerName = '';
  newOfficerEmail = '';
  newOfficerPhone = '';
  newOfficerCollegeId = 1;

  // Companies & HRs
  companies = signal<any[]>([]);
  pendingHRs = signal<any[]>([]);
  myHRProfile = signal<any>(null);
  showAddCompanyModal = signal(false);
  newCompanyName = '';
  newCompanyEmail = '';
  newCompanyIndustry = '';
  newCompanyWebsite = '';
  newCompanyAddress = '';

  // Students
  students = signal<any[]>([]);
  myStudentProfile = signal<any>(null);
  showEnrollModal = signal(false);
  newStudentName = '';
  newStudentEmail = '';
  newStudentPassword = 'Student@123';
  newStudentRegNo = '';
  newStudentDept = 'Computer Science';
  newStudentDegree = 'B.Tech';
  newStudentGradYear = 2026;
  newStudentCGPA = 8.5;
  newStudentBacklogs = 0;

  // Jobs, Rounds & Drives
  jobs = signal<any[]>([]);
  showCreateJobModal = signal(false);
  newJobTitle = '';
  newJobLocation = 'Chennai';
  newJobPackage = 800000;
  newJobMinCGPA = 7.0;
  newJobMaxArrears = 0;
  newJobOpenToAll = true;
  newJobDesc = '';

  selectedJobRounds = signal<any[] | null>(null);
  selectedJobDrives = signal<any[] | null>(null);
  currentJobId = signal<number>(0);

  // Applications
  applications = signal<any[]>([]);
  filterJobId = '1';
  currentHistory = signal<any[] | null>(null);
  selectedAppId = signal<number>(0);

  // Schedules
  schedules = signal<any[]>([]);
  showScheduleModal = signal(false);
  newScheduleAppId = 1;
  newScheduleRoundId = 1;
  newScheduleInterviewer = 'Technical Team';
  newScheduleStartTime = '';
  newScheduleEndTime = '';
  newScheduleMeetingLink = 'https://meet.google.com/xyz-placement';

  showFeedbackModal = signal(false);
  activeScheduleId = 0;
  feedbackResult = 1;
  feedbackRating = 8;
  feedbackRemarks = 'Good candidate';

  // Security
  currentPassword = '';
  newPassword = '';

  // Console
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

  isSuperAdmin(): boolean {
    const r = this.userRole();
    return r === 'SuperAdmin' || r === 'Admin';
  }

  setTab(tab: 'dashboard' | 'colleges' | 'companies' | 'students' | 'jobs' | 'applications' | 'interviews' | 'account' | 'api-console') {
    this.activeTab.set(tab);
    if (tab === 'dashboard') this.loadReports();
    if (tab === 'colleges') { this.loadColleges(); this.loadOfficers(); }
    if (tab === 'companies') { this.loadCompanies(); if (this.isSuperAdmin()) this.loadPendingHRs(); if (this.userRole() === 'CompanyHR') this.loadMyHR(); }
    if (tab === 'students') { this.loadStudents(); if (this.userRole() === 'Student') this.loadMyStudent(); }
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
        this.setTab('dashboard');
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
    this.collegeReport.set(null);
    this.companyReport.set(null);
    this.studentReport.set(null);
  }

  loadReports() {
    if (this.isSuperAdmin()) {
      this.http.get(`${this.apiUrl()}/api/reports/platform-overview`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.platformReport.set(data), error: () => {} });
    }
    if (this.userRole() === 'PlacementOfficer') {
      this.http.get(`${this.apiUrl()}/api/reports/my-college`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.collegeReport.set(data), error: () => {} });
    }
    if (this.userRole() === 'CompanyHR') {
      this.http.get(`${this.apiUrl()}/api/reports/my-company`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.companyReport.set(data), error: () => {} });
    }
    if (this.userRole() === 'Student') {
      this.http.get(`${this.apiUrl()}/api/reports/my-summary`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.studentReport.set(data), error: () => {} });
    }
  }

  // Colleges CRUD
  loadColleges() {
    this.http.get<any[]>(`${this.apiUrl()}/api/colleges`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.colleges.set(data), error: () => this.colleges.set([]) });
  }

  createCollege() {
    if (!this.newCollegeName || !this.newCollegeCity) {
      this.collegeError.set('Name & City required.');
      return;
    }
    this.collegeSaving.set(true);
    this.http.post(`${this.apiUrl()}/api/colleges`, {
      collegeName: this.newCollegeName,
      city: this.newCollegeCity,
      email: this.newCollegeEmail,
      phoneNumber: this.newCollegePhone,
      address: this.newCollegeAddress
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.collegeSaving.set(false);
        this.showAddCollegeModal.set(false);
        this.newCollegeName = '';
        this.newCollegeCity = '';
        this.loadColleges();
      },
      error: (err) => {
        this.collegeSaving.set(false);
        this.collegeError.set(err.error?.message || 'Failed to save.');
      }
    });
  }

  deleteCollege(id: number) {
    if (!confirm('Delete college #' + id + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/colleges/${id}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadColleges(), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  // Placement Officers CRUD
  loadOfficers() {
    this.http.get<any[]>(`${this.apiUrl()}/api/placementofficers`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.placementOfficers.set(data), error: () => this.placementOfficers.set([]) });
  }

  createOfficer() {
    if (!this.newOfficerName || !this.newOfficerEmail) return;
    this.http.post(`${this.apiUrl()}/api/placementofficers`, {
      fullName: this.newOfficerName,
      email: this.newOfficerEmail,
      phoneNumber: this.newOfficerPhone,
      collegeId: this.newOfficerCollegeId
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.showAddOfficerModal.set(false);
        this.newOfficerName = '';
        this.newOfficerEmail = '';
        this.loadOfficers();
      },
      error: (err) => alert(err.error?.message || 'Create officer failed.')
    });
  }

  deleteOfficer(id: number) {
    if (!confirm('Delete officer #' + id + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/placementofficers/${id}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadOfficers(), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  // Companies & HRs CRUD
  loadCompanies() {
    this.http.get<any[]>(`${this.apiUrl()}/api/companies`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.companies.set(data), error: () => this.companies.set([]) });
  }

  createCompany() {
    if (!this.newCompanyName || !this.newCompanyEmail) return;
    this.http.post(`${this.apiUrl()}/api/companies`, {
      companyName: this.newCompanyName,
      companyEmail: this.newCompanyEmail,
      industry: this.newCompanyIndustry || 'Technology',
      websiteUrl: this.newCompanyWebsite,
      address: this.newCompanyAddress || 'Bangalore'
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.showAddCompanyModal.set(false);
        this.newCompanyName = '';
        this.newCompanyEmail = '';
        this.loadCompanies();
      },
      error: (err) => alert(err.error?.message || 'Save company failed.')
    });
  }

  deleteCompany(id: number) {
    if (!confirm('Delete company #' + id + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/companies/${id}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadCompanies(), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  loadPendingHRs() {
    this.http.get<any[]>(`${this.apiUrl()}/api/company-hrs/pending`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.pendingHRs.set(data), error: () => this.pendingHRs.set([]) });
  }

  approveHR(id: number) {
    this.http.put(`${this.apiUrl()}/api/company-hrs/${id}/approve`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => { alert('HR approved!'); this.loadPendingHRs(); }, error: (err) => alert(err.error?.message || 'Error.') });
  }

  rejectHR(id: number) {
    this.http.put(`${this.apiUrl()}/api/company-hrs/${id}/reject`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => { alert('HR rejected.'); this.loadPendingHRs(); }, error: (err) => alert(err.error?.message || 'Error.') });
  }

  loadMyHR() {
    this.http.get(`${this.apiUrl()}/api/company-hrs/me`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.myHRProfile.set(data), error: () => {} });
  }

  // Students CRUD
  loadStudents() {
    this.http.get<any[]>(`${this.apiUrl()}/api/students`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.students.set(data), error: () => this.students.set([]) });
  }

  loadPendingStudents() {
    this.http.get<any[]>(`${this.apiUrl()}/api/students/pending`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.students.set(data), error: () => this.students.set([]) });
  }

  loadMyStudent() {
    this.http.get(`${this.apiUrl()}/api/students/me`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.myStudentProfile.set(data), error: () => {} });
  }

  enrollStudent() {
    if (!this.newStudentName || !this.newStudentEmail || !this.newStudentRegNo) return;
    this.http.post(`${this.apiUrl()}/api/students/enroll`, {
      fullName: this.newStudentName,
      email: this.newStudentEmail,
      password: this.newStudentPassword,
      registerNumber: this.newStudentRegNo,
      department: this.newStudentDept,
      degree: this.newStudentDegree,
      graduationYear: this.newStudentGradYear,
      cgpa: this.newStudentCGPA,
      activeBacklogs: this.newStudentBacklogs
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.showEnrollModal.set(false);
        this.loadStudents();
      },
      error: (err) => alert(err.error?.message || 'Enroll failed.')
    });
  }

  approveStudent(id: number) {
    this.http.put(`${this.apiUrl()}/api/students/${id}/approve`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadStudents(), error: (err) => alert(err.error?.message || 'Error.') });
  }

  rejectStudent(id: number) {
    this.http.put(`${this.apiUrl()}/api/students/${id}/reject`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadStudents(), error: (err) => alert(err.error?.message || 'Error.') });
  }

  suspendStudent(id: number) {
    this.http.put(`${this.apiUrl()}/api/students/${id}/suspend`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadStudents(), error: (err) => alert(err.error?.message || 'Error.') });
  }

  // Jobs, Rounds & Drives
  loadJobs() {
    this.http.get<any[]>(`${this.apiUrl()}/api/jobs`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.jobs.set(data), error: () => this.jobs.set([]) });
  }

  loadEligibleJobs() {
    this.http.get<any[]>(`${this.apiUrl()}/api/jobs/eligible`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.jobs.set(data), error: () => this.jobs.set([]) });
  }

  createJob() {
    if (!this.newJobTitle) return;
    this.http.post(`${this.apiUrl()}/api/jobs`, {
      title: this.newJobTitle,
      description: this.newJobDesc,
      location: this.newJobLocation,
      salaryPackage: this.newJobPackage,
      minCgpa: this.newJobMinCGPA,
      maxArrearsAllowed: this.newJobMaxArrears,
      isOpenToAllColleges: this.newJobOpenToAll
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.showCreateJobModal.set(false);
        this.loadJobs();
      },
      error: (err) => alert(err.error?.message || 'Post job failed.')
    });
  }

  publishJob(id: number) {
    this.http.put(`${this.apiUrl()}/api/jobs/${id}/publish`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadJobs(), error: (err) => alert(err.error?.message || 'Publish failed.') });
  }

  closeJob(id: number) {
    this.http.put(`${this.apiUrl()}/api/jobs/${id}/close`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadJobs(), error: (err) => alert(err.error?.message || 'Close failed.') });
  }

  deleteJob(id: number) {
    if (!confirm('Delete draft job #' + id + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/jobs/${id}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadJobs(), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  checkEligibility(id: number) {
    this.http.get<any>(`${this.apiUrl()}/api/jobs/${id}/eligibility`, { headers: this.getHeaders() })
      .subscribe({
        next: (res) => alert(res.isEligible ? '✅ You are ELIGIBLE for this job!' : '❌ Not eligible: ' + (res.reasons?.join(', ') || 'Requirements unmet')),
        error: (err) => alert(err.error?.message || 'Check failed.')
      });
  }

  applyJob(jobId: number) {
    this.http.post(`${this.apiUrl()}/api/jobs/${jobId}/apply`, {}, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          alert('Successfully applied to job #' + jobId);
          this.setTab('applications');
        },
        error: (err) => alert(err.error?.message || 'Apply failed.')
      });
  }

  viewRounds(jobId: number) {
    this.currentJobId.set(jobId);
    this.http.get<any[]>(`${this.apiUrl()}/api/jobs/${jobId}/interview-rounds`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.selectedJobRounds.set(data), error: () => this.selectedJobRounds.set([]) });
  }

  openAddRound(jobId: number) {
    const roundName = prompt('Round Name (e.g. Technical Interview 1):', 'Technical Interview');
    if (!roundName) return;
    this.http.post(`${this.apiUrl()}/api/jobs/${jobId}/interview-rounds`, {
      roundNumber: 1,
      roundName: roundName,
      roundType: 'Technical',
      description: 'Core concepts interview'
    }, { headers: this.getHeaders() }).subscribe({
      next: () => { alert('Round added!'); this.viewRounds(jobId); },
      error: (err) => alert(err.error?.message || 'Add round failed.')
    });
  }

  deleteRound(roundId: number) {
    if (!confirm('Delete round #' + roundId + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/interview-rounds/${roundId}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.viewRounds(this.currentJobId()), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  viewDrives(jobId: number) {
    this.currentJobId.set(jobId);
    this.http.get<any[]>(`${this.apiUrl()}/api/recruitment-drives/job/${jobId}`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.selectedJobDrives.set(data), error: () => this.selectedJobDrives.set([]) });
  }

  openAddDrive(jobId: number) {
    const driveName = prompt('Drive Name (e.g. Campus Onboarding 2026):', 'Campus Placement Drive');
    if (!driveName) return;
    this.http.post(`${this.apiUrl()}/api/recruitment-drives`, {
      jobId: jobId,
      driveName: driveName,
      driveDate: new Date().toISOString(),
      venue: 'Main Auditorium'
    }, { headers: this.getHeaders() }).subscribe({
      next: () => { alert('Drive scheduled!'); this.viewDrives(jobId); },
      error: (err) => alert(err.error?.message || 'Create drive failed.')
    });
  }

  cancelDrive(driveId: number) {
    this.http.put(`${this.apiUrl()}/api/recruitment-drives/${driveId}/cancel`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.viewDrives(this.currentJobId()), error: (err) => alert(err.error?.message || 'Cancel failed.') });
  }

  deleteDrive(driveId: number) {
    if (!confirm('Delete drive #' + driveId + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/recruitment-drives/${driveId}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.viewDrives(this.currentJobId()), error: (err) => alert(err.error?.message || 'Delete failed.') });
  }

  // Applications & History
  loadApplications() {
    if (this.userRole() === 'Student') {
      this.http.get<any[]>(`${this.apiUrl()}/api/job-applications/my-applications`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.applications.set(data), error: () => this.applications.set([]) });
    } else {
      const jid = this.filterJobId || '1';
      this.http.get<any[]>(`${this.apiUrl()}/api/jobs/${jid}/applications`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.applications.set(data), error: () => this.applications.set([]) });
    }
  }

  shortlistApp(id: number) {
    this.http.put(`${this.apiUrl()}/api/applications/${id}/shortlist`, { remarks: 'Shortlisted for technical round' }, { headers: this.getHeaders() })
      .subscribe({ next: () => { alert('Shortlisted application #' + id); this.loadApplications(); }, error: (err) => alert(err.error?.message || 'Shortlist failed.') });
  }

  rejectApp(id: number) {
    this.http.put(`${this.apiUrl()}/api/applications/${id}/reject`, { remarks: 'Does not meet criteria' }, { headers: this.getHeaders() })
      .subscribe({ next: () => { alert('Rejected application #' + id); this.loadApplications(); }, error: (err) => alert(err.error?.message || 'Reject failed.') });
  }

  withdrawApp(id: number) {
    if (!confirm('Withdraw application #' + id + '?')) return;
    this.http.put(`${this.apiUrl()}/api/applications/${id}/withdraw`, {}, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadApplications(), error: (err) => alert(err.error?.message || 'Withdraw failed.') });
  }

  loadHistory(appId: number) {
    this.selectedAppId.set(appId);
    this.http.get<any[]>(`${this.apiUrl()}/api/applications/${appId}/history`, { headers: this.getHeaders() })
      .subscribe({ next: (data) => this.currentHistory.set(data), error: () => this.currentHistory.set([]) });
  }

  // Interview Schedules
  loadInterviews() {
    if (this.userRole() === 'Student') {
      this.http.get<any[]>(`${this.apiUrl()}/api/interview-schedules/my-schedules`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.schedules.set(data), error: () => this.schedules.set([]) });
    } else {
      this.http.get<any[]>(`${this.apiUrl()}/api/interview-schedules/round/1`, { headers: this.getHeaders() })
        .subscribe({ next: (data) => this.schedules.set(data), error: () => this.schedules.set([]) });
    }
  }

  openScheduleModal(appId: number, jobId: number) {
    this.newScheduleAppId = appId;
    const now = new Date();
    now.setHours(now.getHours() + 2);
    this.newScheduleStartTime = now.toISOString().slice(0, 16);
    now.setHours(now.getHours() + 1);
    this.newScheduleEndTime = now.toISOString().slice(0, 16);
    this.showScheduleModal.set(true);
  }

  submitSchedule() {
    this.http.post(`${this.apiUrl()}/api/interview-schedules`, {
      applicationId: this.newScheduleAppId,
      roundId: this.newScheduleRoundId,
      startTime: new Date(this.newScheduleStartTime).toISOString(),
      endTime: new Date(this.newScheduleEndTime).toISOString(),
      meetingLink: this.newScheduleMeetingLink,
      interviewerName: this.newScheduleInterviewer
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        alert('Interview scheduled successfully!');
        this.showScheduleModal.set(false);
        this.setTab('interviews');
      },
      error: (err) => alert(err.error?.message || 'Schedule failed.')
    });
  }

  openFeedback(scheduleId: number) {
    this.activeScheduleId = scheduleId;
    this.showFeedbackModal.set(true);
  }

  submitFeedback() {
    this.http.put(`${this.apiUrl()}/api/interviews/${this.activeScheduleId}/feedback`, {
      result: Number(this.feedbackResult),
      rating: Number(this.feedbackRating),
      remarks: this.feedbackRemarks
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        alert('Feedback submitted!');
        this.showFeedbackModal.set(false);
        this.loadInterviews();
      },
      error: (err) => alert(err.error?.message || 'Submit feedback failed.')
    });
  }

  deleteSchedule(scheduleId: number) {
    if (!confirm('Cancel schedule #' + scheduleId + '?')) return;
    this.http.delete(`${this.apiUrl()}/api/interviews/${scheduleId}`, { headers: this.getHeaders() })
      .subscribe({ next: () => this.loadInterviews(), error: (err) => alert(err.error?.message || 'Cancel failed.') });
  }

  // Password Change
  changePassword() {
    if (!this.currentPassword || !this.newPassword) return;
    this.http.post(`${this.apiUrl()}/api/auth/change-password`, {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }, { headers: this.getHeaders() }).subscribe({
      next: () => {
        alert('Password changed successfully!');
        this.currentPassword = '';
        this.newPassword = '';
      },
      error: (err) => alert(err.error?.message || 'Change password failed.')
    });
  }

  // Interactive Console
  runConsole() {
    this.consoleLoading.set(true);
    let req$;
    const url = `${this.apiUrl()}${this.consoleEndpoint}`;
    const opts = { headers: this.getHeaders(), observe: 'response' as const };

    if (this.consoleMethod === 'GET') {
      req$ = this.http.get(url, opts);
    } else if (this.consoleMethod === 'POST') {
      let body = {};
      try { body = JSON.parse(this.consoleBody); } catch {}
      req$ = this.http.post(url, body, opts);
    } else if (this.consoleMethod === 'PUT') {
      let body = {};
      try { body = JSON.parse(this.consoleBody); } catch {}
      req$ = this.http.put(url, body, opts);
    } else {
      req$ = this.http.delete(url, opts);
    }

    req$.subscribe({
      next: (resp: any) => {
        this.consoleLoading.set(false);
        this.consoleStatus.set(resp.status);
        this.consoleResponse.set(resp.body);
      },
      error: (err) => {
        this.consoleLoading.set(false);
        this.consoleStatus.set(err.status);
        this.consoleResponse.set(err.error || { error: err.message });
      }
    });
  }

  getStatusName(status: number): string {
    const map: Record<number, string> = {
      0: 'Applied',
      1: 'Under Review',
      2: 'Shortlisted',
      3: 'Interview Scheduled',
      4: 'Interview In Progress',
      5: 'Selected / Offered',
      6: 'Rejected',
      7: 'Withdrawn'
    };
    return map[status] || 'Status #' + status;
  }

  getStatusBadge(status: number): string {
    if (status === 2 || status === 5) return 'badge-green';
    if (status === 6 || status === 7) return 'badge-red';
    if (status === 3 || status === 4) return 'badge-blue';
    return 'badge-yellow';
  }
}
