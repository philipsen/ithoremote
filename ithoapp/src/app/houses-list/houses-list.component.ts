import { Component, OnInit } from '@angular/core';
import { House } from '../house';

@Component({
  selector: 'app-houses-list',
  templateUrl: './houses-list.component.html',
  styleUrls: ['./houses-list.component.sass']
})
export class HousesListComponent implements OnInit {

  houses: House[];

  constructor(rivate houseService: HousesService) { }

  ngOnInit() {
    this.getHouses();
  }

  getHouses(): void {
    this.houseService.getHouses()
      .subscribe(houses => this.houses = houses);
  }
}
