import { Component, OnInit } from '@angular/core';
import { House } from '../models/house';
import { HousesService } from '../services/houses.service';

@Component({
  selector: 'app-houses-list',
  templateUrl: './houses-list.component.html',
  styleUrls: ['./houses-list.component.sass']
})
export class HousesListComponent implements OnInit {

  houses: House[];

  constructor(private houseService: HousesService) { }

  ngOnInit() {
    this.getHouses();
  }

  getHouses(): void {
    this.houseService.getHouses()
      .subscribe(houses => this.houses = houses);
  }

}
