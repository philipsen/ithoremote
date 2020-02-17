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

  public state: string;

  private url = 'http://localhost:5000/api/';
  private hubConnection: signalR.HubConnection;

  constructor( private http: HttpClient) {
    this.startConnection();
    this.startSubscription();
  }

  public startConnection = () => {
    console.log('startConnection');
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl('http://localhost:5000/ithoHub')
                            .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));


  }

  private startSubscription() {
    this.hubConnection.on('state', (data) => {
      // this.data = data;
      console.log(data);
      this.state = data.ventilation.case;
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

