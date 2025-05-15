import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { RegisterComponent } from './features/register/register.component';
import { UserProfileComponent } from './features/userProfile/userProfile.component';
import { AuthGuard } from './core/guards/auth.guard';
import { NotFoundComponent } from './features/notFound/notFound.component';
import { CalendarComponent } from './features/calendar/calendar.component';
import { HomepageComponent } from './features/homepage/homepage.component';
import { DeclarationGeneratorComponent } from './features/declaration-generator/declaration-generator.component';

const routes: Routes = [
  // Public routes
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  // Protected routes
  { 
    path: '', 
    component: HomepageComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'user-profile', 
    component: UserProfileComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'calendar', 
    component: CalendarComponent, 
    canActivate: [AuthGuard] 
  },
  {
    path: 'declaration-generator',
    component: DeclarationGeneratorComponent,
    canActivate: [AuthGuard]
  },
  
  // Fallback route
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }