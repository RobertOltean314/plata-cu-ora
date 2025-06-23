import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './features/login/login.component';
import { RegisterComponent } from './features/register/register.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { UserProfileComponent } from './features/userProfile/userProfile.component';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './core/navbar/navbar.component';
import { DeclarationGeneratorComponent } from './features/declaration-generator/declaration-generator.component';
import { ReactiveFormsModule } from '@angular/forms';
import { OrarComponent } from './features/orar/orar.component';
import { CalendarComponent } from './features/calendar/calendar.component';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    UserProfileComponent,
    NavbarComponent,
    DeclarationGeneratorComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    CommonModule,
    ReactiveFormsModule,
    OrarComponent,
    CalendarComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }