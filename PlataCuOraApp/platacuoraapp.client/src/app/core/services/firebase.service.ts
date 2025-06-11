import { Injectable } from '@angular/core';
import { initializeApp } from 'firebase/app';
import { getAuth, Auth } from 'firebase/auth';
import { environment } from '../../environment/environment';

@Injectable({
  providedIn: 'root'
})
export class FirebaseService {
  private app = initializeApp(environment.firebase);
  public auth: Auth = getAuth(this.app);

  constructor() { }
}
