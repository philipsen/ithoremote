import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HousesListComponent } from './houses-list/houses-list.component';
import { HouseDetailComponent } from './house-detail/house-detail.component';
import { IthoButtonComponent } from './house-detail/itho-button/itho-button.component';
import { ConfigLoaderService } from './services/config-loader.service';
import { PreloadFactory } from './services/preload-service.factory';
import { GaugeChartModule } from 'angular-gauge-chart';
import { HousesAdminComponent } from './houses-admin/houses-admin.component';
import { AdminItemComponent } from './houses-admin/admin-item/admin-item.component';

@NgModule({
  declarations: [
    AppComponent,
    HousesListComponent,
    HouseDetailComponent,
    IthoButtonComponent,
    HousesAdminComponent,
    AdminItemComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    GaugeChartModule
  ],
  providers: [
    ConfigLoaderService,
    {
      provide: APP_INITIALIZER,
      deps: [
        ConfigLoaderService
      ],
      multi: true,
      useFactory: PreloadFactory
    }

  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
