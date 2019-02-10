import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { House } from '../models/house';

@Injectable({
  providedIn: 'root'
})

export class HousesService {
  private url = 'http://localhost:5000/api/';

  constructor( private http: HttpClient) { }

  getHouses(): Observable<House[]> {
    return this.http.get<House[]>(this.url + 'houses');
  }
}

