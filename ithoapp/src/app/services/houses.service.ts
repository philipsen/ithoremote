import { Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { House } from '../models/house';
import { FanState } from '../models/fanstate';
import { Wmt40Buttons, Wmt6Buttons } from './houses.hardcodedbuttons';
import { IthoButton } from '../models/itho-button';
import { ConfigLoaderService } from './config-loader.service';

import * as signalR from '@microsoft/signalr';
import { Transponder } from '../models/transponders';

@Injectable({
  providedIn: 'root'
})

export class HousesService implements OnInit {
  public house: House = {
    fanspeed: 0,
    state: ''
  };
  private hubConnection: signalR.HubConnection;
  public url: string;
  public fanstates: FanState[] = [];
  transponders: Transponder[] = [];
  public connected = false;

  constructor(private http: HttpClient, private configLoaderService: ConfigLoaderService) {
    this.startConnection();
  }

  ngOnInit() {
    this.url = this.configLoaderService.apiUrl;
  }

  public startConnection = () => {
    console.log('startConnection ' + this.configLoaderService.apiUrl);
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.configLoaderService.apiUrl + '/ithoHub')
      .withAutomaticReconnect([1000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000])
      .build();
    this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started');
        this.connected = true;
      })
      .catch(err => {
        console.log('Error while starting connection: ' + err);
        this.connected = false;
      });
    this.hubConnection.onreconnecting(error => {
      console.log(error);
      this.connected = false;
    });
    this.hubConnection.onreconnected(() => {
      console.log('reconnected');
      this.connected = true;
    });
  }

  public startSubscription(house: string) {
    const subName = 'state/' + house;
    this.hubConnection.on(subName, (data) => {
      // console.log('nd = ' + data);
      const obj = JSON.parse(data);
      this.house.state = obj.state;
      this.house.fanspeed = obj.fanspeed;
    });
  }

  public startFanstateSubscription() {
    console.log('startFanstateSubscription');
    const subName = 'fanstates';
    this.hubConnection.on(subName, (data: string) => {
      // console.log('fs = ' + data);
      const obj = JSON.parse(data);
      const res = [];
      // tslint:disable-next-line:forin
      for (const st in obj['state']) {
        obj['state'][st].id = st;
        res.push(obj.state[st]);
      }
      this.fanstates = res;
    });
  }

  public startTransponderSubscription() {
    this.hubConnection.on('handheld', (data: string) => {
      // console.log('handheld = ' + data);
      this.transponders.push(JSON.parse(data));
    });
  }

  getHouses(): Observable<House[]> {
    return this.http.get<House[]>(this.configLoaderService.apiUrl + '/api/houses');
  }

  getHouse(id: string): Observable<House> {
    const url = this.configLoaderService.apiUrl + '/api/house/status/' + id;
    return this.http.get<House>(url);
  }

  getButtons(id: String): IthoButton[] {
    let b = Wmt6Buttons;
    switch (id) {
      case 'wmt40':
        b = Wmt40Buttons;
    }
    return b;
  }

}

