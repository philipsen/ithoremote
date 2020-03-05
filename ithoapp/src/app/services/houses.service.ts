import { Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { House } from '../models/house';
import { Wmt40Buttons, Wmt6Buttons } from './houses.hardcodedbuttons';
import { IthoButton } from '../models/itho-button';


import { ConfigLoaderService } from './config-loader.service';

import * as signalR from '@aspnet/signalr';

@Injectable({
  providedIn: 'root'
})

export class HousesService implements OnInit {

  public state: string;
  public fanspeed: Number;

  private hubConnection: signalR.HubConnection;
  public url: string;

  constructor( private http: HttpClient, private configLoaderService: ConfigLoaderService) {
    this.startConnection();
    this.startSubscription();
  }

  ngOnInit() {
    this.url = this.configLoaderService.apiUrl;
  }

  public startConnection = () => {
    console.log('startConnection ' + this.configLoaderService.apiUrl);
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl(this.configLoaderService.apiUrl + '/ithoHub')
                            .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  private startSubscription() {
    this.hubConnection.on('state', (data) => {
      console.log(data);
      let obj = JSON.parse(data);
      this.state = obj.state;
      this.fanspeed = obj.fanspeed;
      console.log('fs = ' + this.fanspeed);
    });
  }

  getHouses(): Observable<House[]> {
    console.log('url = ' + this.configLoaderService.apiUrl);
    return this.http.get<House[]>(this.configLoaderService.apiUrl + '/api/houses');
  }

  getHouse(id: string): Observable<House> {
    const url = this.configLoaderService.apiUrl + '/api/house/' + id;
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

