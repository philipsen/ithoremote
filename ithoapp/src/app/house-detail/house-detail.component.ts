import { Component, OnInit, ViewChild } from '@angular/core';
import { HousesService } from '../services/houses.service';
import { House } from '../models/house';
import { ActivatedRoute } from '@angular/router';
import { IthoButton } from '../models/itho-button';

@Component({
  selector: 'app-house-detail',
  templateUrl: './house-detail.component.html',
  styleUrls: ['./house-detail.component.sass']
})
export class HouseDetailComponent implements OnInit {

  @ViewChild('target', { static: true}) targetElement: any;
  result: string;

  constructor(
    private housesService: HousesService,
    private route: ActivatedRoute
) {
  const id = this.route.snapshot.paramMap.get('id');
  this.housesService.startSubscription(id);
}

  public house: House =  {
    fanspeed: 1,
    state: ''
  };

  buttons: IthoButton[];
  public canvasWidth = 500;
  public needleValue = this.house.fanspeed;
  public centralLabel = '';
  public name = this.house.state;
  public bottomLabel = '';
  // public bottomLabel = '65'
  public options = {
      hasNeedle: true,
      needleColor: 'gray',
      needleUpdateSpeed: 2000,
      arcColors: ['rgb(44, 151, 222)', 'lightgray'],
      arcDelimiters: [30],
      rangeLabel: ['off', 'max'],
      needleStartValue: 0,
  };
  ngOnInit() {
    console.log('init component');
    const width = Math.min(1500, this.targetElement.nativeElement.offsetWidth);
    this.canvasWidth = width - 10;

    this.getHouse();
    this.getButtons();
    setInterval(() => {
      this.canvasWidth = Math.min(1500, this.targetElement.nativeElement.offsetWidth - 10);
      this.options.needleStartValue = this.needleValue.valueOf();
      this.needleValue = this.house.fanspeed;
      this.centralLabel = this.needleValue.toString() + '%';
      this.bottomLabel = this.house.state;
    }, 1000);
  }

  getHouse(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.housesService.getHouse(id)
      .subscribe(house => {
        console.log('hs = ' + house.state + ' ' + house.fanspeed.toFixed());
        this.house = house;
      });
}

getButtons(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.buttons = this.housesService.getButtons(id);
}
}
