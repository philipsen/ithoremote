import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { House } from '../models/house';
import { Wmt40Buttons, Wmt6Buttons } from './houses.hardcodedbuttons';
import { IthoButton } from '../models/itho-button';

@Injectable({
  providedIn: 'root'
})

export class HousesService {

  private url = 'http://localhost:5000/api/';

  constructor( private http: HttpClient) { }

  getHouses(): Observable<House[]> {
    return this.http.get<House[]>(this.url + 'houses');
  }

  getHouse(id: string): Observable<House> {
    const url = this.url + 'house/' + id;
    return this.http.get<House>(url);
  }

  getButtons(id: String): IthoButton[] {
    let b = Wmt6Buttons;
    switch (id) {
        case 'wmt40':
            b = Wmt40Buttons;
    }
    console.log('getButtons', id, b);
    return b;
  }

}

