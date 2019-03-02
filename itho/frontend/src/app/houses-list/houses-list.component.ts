import { Component, OnInit } from '@angular/core';
import { House } from '../models/house';
import { HousesService } from '../services/houses.service';
import { Houses2Service } from '../services/houses2.service';

@Component({
  selector: 'app-houses-list',
  templateUrl: './houses-list.component.html',
  styleUrls: ['./houses-list.component.scss']
})
export class HousesListComponent implements OnInit {

  houses: House[];

  constructor(
    private houseService: HousesService
    ) {
    console.log('houses HousesListComponent ctor');
  }

  ngOnInit() {
    console.log('houses HousesListComponent init');
    this.getHouses();
  }

  getHouses(): void {
    console.log('get houses');
    this.houseService.getHouses()
      .subscribe(houses => this.houses = houses);
  }

}
