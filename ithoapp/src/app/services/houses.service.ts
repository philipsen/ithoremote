import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { House } from '../models/house';
import { Wmt40Buttons, Wmt6Buttons } from './houses.hardcodedbuttons';
import { IthoButton } from '../models/itho-button';

import * as signalR from '@aspnet/signalr';

@Injectable({
  providedIn: 'root'
})

export class HousesService {

  private url = 'https://localhost:5001/api/';
  private hubConnection: signalR.HubConnection;

  constructor( private http: HttpClient) {
    this.startConnection();
  }

  public startConnection = () => {
    console.log('startConnection');
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl('https://localhost:5001/ithoHub')
                            .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));

    this.hubConnection.on('aap', (data) => {
      // this.data = data;
      console.log(data);
    });
  }

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

