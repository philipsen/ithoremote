import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HousesListComponent } from './houses-list/houses-list.component';
import { HouseDetailComponent } from './house-detail/house-detail.component';
import { IthoButtonComponent } from './house-detail/itho-button/itho-button.component';

@NgModule({
  declarations: [
    AppComponent,
    HousesListComponent,
    HouseDetailComponent,
    IthoButtonComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
