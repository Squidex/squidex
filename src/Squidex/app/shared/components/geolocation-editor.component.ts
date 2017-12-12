/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, ElementRef, forwardRef, ViewChild } from '@angular/core';
import { ControlValueAccessor, FormBuilder, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../../framework/utils/types';

import { ResourceLoaderService } from './../../framework/services/resource-loader.service';
import { ValidatorsEx } from './../../framework/angular/validators';

import { AppContext } from './app-context';

declare var L: any;
declare var google: any;

export const SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GeolocationEditorComponent), multi: true
};

interface Geolocation {
    latitude: number;
    longitude: number;
}

@Component({
    selector: 'sqx-geolocation-editor',
    styleUrls: ['./geolocation-editor.component.scss'],
    templateUrl: './geolocation-editor.component.html',
    providers: [SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class GeolocationEditorComponent implements ControlValueAccessor, AfterViewInit {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private marker: any;
    private map: any;
    private searchBox: any;
    private value: Geolocation | null = null;
    public useGoogleMaps = false;

    public get hasValue() {
        return !!this.value;
    }

    public geolocationForm =
    this.formBuilder.group({
        latitude: [
            '',
            [
                ValidatorsEx.between(-90, 90)
            ]
        ],
        longitude: [
            '',
            [
                ValidatorsEx.between(-180, 180)
            ]
        ]
    });

    @ViewChild('editor')
    public editor: ElementRef;

    @ViewChild('searchBox')
    public searchBoxInput: ElementRef;

    public isDisabled = false;

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder,
        private readonly ctx: AppContext
    ) {
        this.useGoogleMaps = this.ctx.uiSettings.geocoderKey !== '';
    }

    public writeValue(value: Geolocation) {
        if (Types.isObject(value) && Types.isNumber(value.latitude) && Types.isNumber(value.longitude)) {
            this.value = value;
        } else {
            this.value = null;
        }

        if (this.marker || (!this.marker && this.map && this.value)) {
            this.updateMarker(true, false);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        if (!this.useGoogleMaps) {
            this.setDisabledStateOSM(isDisabled);
        } else {
            this.setDisabledStateGoogle(isDisabled);
        }
    }

    private setDisabledStateOSM(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (isDisabled) {
            if (this.map) {
                this.map.zoomControl.disable();

                this.map._handlers.forEach((handler: any) => {
                    handler.disable();
                });
            }

            if (this.marker) {
                this.marker.dragging.disable();
            }

            this.geolocationForm.disable();
        } else {
            if (this.map) {
                this.map.zoomControl.enable();

                this.map._handlers.forEach((handler: any) => {
                    handler.enable();
                });
            }

            if (this.marker) {
                this.marker.dragging.enable();
            }

            this.geolocationForm.enable();
        }
    }

    private setDisabledStateGoogle(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (isDisabled) {
            if (this.map) {
                this.map.setOptions({
                    draggable: false,
                    zoomControl: false
                });
            }

            if (this.marker) {
                this.marker.setDraggable(false);
            }

            this.geolocationForm.disable();
            this.searchBoxInput.nativeElement.disabled = true;
        } else {
            if (this.map) {
                this.map.setOptions({
                    draggable: true,
                    zoomControl: true
                });
            }

            if (this.marker) {
                this.marker.setDraggable(true);
            }

            this.geolocationForm.enable();
            this.searchBoxInput.nativeElement.disabled = false;
        }
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public updateValueByInput() {
        let updateMap = this.geolocationForm.controls['latitude'].value !== null &&
            this.geolocationForm.controls['longitude'].value !== null;

        this.value = this.geolocationForm.value;

        if (updateMap) {
            this.updateMarker(true, true);
        } else {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    public ngAfterViewInit() {
        if (!this.useGoogleMaps) {
            this.ngAfterViewInitOSM();
        } else {
            this.ngAfterViewInitGoogle();
        }
    }

    private ngAfterViewInitOSM() {
        this.searchBoxInput.nativeElement.remove();

        this.resourceLoader.loadStyle('https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.0.3/leaflet.css');
        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.0.3/leaflet.js').then(
            () => {
                this.map = L.map(this.editor.nativeElement).fitWorld();

                L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png',
                    {
                        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                    }).addTo(this.map);

                this.map.on('click',
                    (event: any) => {
                        if (!this.marker && !this.isDisabled) {
                            const latlng = event.latlng.wrap();

                            this.value = {
                                latitude: latlng.lat,
                                longitude: latlng.lng
                            };

                            this.updateMarker(false, true);
                        }
                    });

                this.updateMarker(true, false);

                if (this.isDisabled) {
                    this.map.zoomControl.disable();

                    this.map._handlers.forEach((handler: any) => {
                        handler.disable();
                    });
                }
            });
    }

    private ngAfterViewInitGoogle() {
        this.resourceLoader.loadScript(`https://maps.googleapis.com/maps/api/js?key=${this.ctx.uiSettings.geocoderKey}&libraries=places`).then(
            () => {
                this.map = new google.maps.Map(this.editor.nativeElement,
                    {
                        zoom: 1,
                        mapTypeControl: false,
                        streetViewControl: false,
                        center: { lat: 0, lng: 0 }
                    });

                this.searchBox = new google.maps.places.SearchBox(this.searchBoxInput.nativeElement);
                this.map.controls[google.maps.ControlPosition.LEFT_TOP].push(this.searchBoxInput.nativeElement);

                this.map.addListener('click',
                    (event: any) => {
                        if (!this.isDisabled) {
                            this.value = {
                                latitude: event.latLng.lat(),
                                longitude: event.latLng.lng()
                            };

                            this.updateMarker(false, true);
                        }
                    });

                this.map.addListener('bounds_changed', (event: any) => {
                    this.searchBox.setBounds(this.map.getBounds());
                });

                this.searchBox.addListener('places_changed', (event: any) => {
                    let places = this.searchBox.getPlaces();

                    if (places.length === 1) {
                        let place = places[0];

                        if (!place.geometry) {
                            console.log('Returned place contains no geometry');
                            return;
                        }

                        if (!this.isDisabled) {
                            let latitude = place.geometry.location.lat();
                            let longitude = place.geometry.location.lng();

                            this.value = { latitude: latitude, longitude: longitude };

                            this.updateMarker(false, true);
                        }
                    }
                });

                this.updateMarker(true, false);

                if (this.isDisabled) {
                    this.map.setOptions({
                        draggable: false,
                        zoomControl: false
                    });
                }
            });
    }

    public reset() {
        this.value = null;
        this.searchBoxInput.nativeElement.value = null;

        this.updateMarker(true, true);
    }

    private updateMarker(zoom: boolean, fireEvent: boolean) {
        if (!this.useGoogleMaps) {
            this.updateMarkerOSM(zoom, fireEvent);
        } else {
            this.updateMarkerGoogle(zoom, fireEvent);
        }
    }

    private updateMarkerOSM(zoom: boolean, fireEvent: boolean) {
        if (this.value) {
            if (!this.marker) {
                this.marker = L.marker([0, 90], { draggable: true }).addTo(this.map);

                this.marker.on('drag', (event: any) => {
                    const latlng = event.latlng.wrap();

                    this.value = {
                        latitude: latlng.lat,
                        longitude: latlng.lng};
                });

                this.marker.on('dragend', () => {
                    this.updateMarker(false, true);
                });

                if (this.isDisabled) {
                    this.marker.dragging.disable();
                }
            }

            const latLng = L.latLng(this.value.latitude, this.value.longitude);

            if (zoom) {
                this.map.setView(latLng, 8);
            } else {
                this.map.panTo(latLng);
            }

            this.marker.setLatLng(latLng);

            this.geolocationForm.setValue(this.value, { emitEvent: false, onlySelf: false });
        } else {
            if (this.marker) {
                this.marker.removeFrom(this.map);
                this.marker = null;
            }

            this.map.fitWorld();

            this.geolocationForm.reset(undefined, { emitEvent: false, onlySelf: false });
        }

        if (fireEvent) {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    private updateMarkerGoogle(zoom: boolean, fireEvent: boolean) {
        if (this.value) {
            if (!this.marker) {
                this.marker =  new google.maps.Marker({
                    position: { lat: 0, lng: 0 },
                    map: this.map,
                    draggable: true
                });

                this.marker.addListener('drag', (event: any) => {
                    if (!this.isDisabled) {
                        this.value = {
                            latitude: event.latLng.lat(),
                            longitude: event.latLng.lng()
                        };
                    }
                });
                this.marker.addListener('dragend', (event: any) => {
                    if (!this.isDisabled) {
                        this.value = {
                            latitude: event.latLng.lat(),
                            longitude: event.latLng.lng()
                        };

                        this.updateMarker(false, true);
                    }
                });
            }

            const latLng = { lat: this.value.latitude, lng: this.value.longitude };

            if (zoom) {
                this.map.setCenter(latLng);
            } else {
                this.map.panTo(latLng);
            }

            this.marker.setPosition(latLng);
            this.map.setZoom(16);

            this.geolocationForm.setValue(this.value, { emitEvent: false, onlySelf: false });
        } else {
            if (this.marker) {
                this.marker.setMap(null);
                this.marker = null;
            }

            this.map.setCenter({ lat: 0, lng: 0 });
            this.map.setZoom(1);

            this.geolocationForm.reset(undefined, { emitEvent: false, onlySelf: false });
        }

        if (fireEvent) {
            this.callChange(this.value);
            this.callTouched();
        }
    }
}