import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { RegisterComponent } from './features/register/register.component';
import { UserProfileComponent } from './features/userProfile/userProfile.component';
import { AuthGuard } from './core/guards/auth.guard';
import { NotFoundComponent } from './features/notFound/notFound.component';
import { CalendarComponent } from './features/calendar/calendar.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'user-profile', component: UserProfileComponent, canActivate: [AuthGuard] }, 
  { path: 'calendar', component: CalendarComponent },
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
