/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, ViewChild } from '@angular/core';
import { FormBuilder, NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    ResourceLoaderService,
    StatefulControlComponent,
    Types,
    UIOptions,
    ValidatorsEx
} from '@app/shared/internal';

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
    providers: [SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GeolocationEditorComponent extends StatefulControlComponent<any, Geolocation> implements AfterViewInit {
    private readonly isGoogleMaps: boolean;
    private marker: any;
    private map: any;
    private value: Geolocation | null = null;

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

    @ViewChild('editor', { static: false })
    public editor: ElementRef<HTMLElement>;

    @ViewChild('searchBox', { static: false })
    public searchBoxInput: ElementRef<HTMLInputElement>;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder,
        private readonly uiOptions: UIOptions
    ) {
        super(changeDetector, {});

        this.isGoogleMaps = uiOptions.get('map.type');
    }

    public writeValue(obj: any) {
        if (Types.isObject(obj) && Types.isNumber(obj.latitude) && Types.isNumber(obj.longitude)) {
            this.value = obj;
        } else {
            this.value = null;
        }

        if (this.marker || (!this.marker && this.map && this.value)) {
            this.updateMarker(true, false);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (!this.snapshot.isGoogleMaps) {
            this.setDisabledStateOSM(isDisabled);
        } else {
            this.setDisabledStateGoogle(isDisabled);
        }

        if (isDisabled) {
            this.geolocationForm.disable();
        } else {
            this.geolocationForm.enable();
        }
    }

    private setDisabledStateOSM(isDisabled: boolean): void {
        const update: (t: any) => any =
            isDisabled ?
            x => x.enable() :
            x => x.disable();

        if (this.map) {
            update(this.map.zoomControl);

            this.map._handlers.forEach((handler: any) => {
                update(handler);
            });
        }

        if (this.marker) {
            update(this.marker.dragging);
        }
    }

    private setDisabledStateGoogle(isDisabled: boolean): void {
        const enabled = !isDisabled;

        if (this.map) {
            this.map.setOptions({ draggable: enabled, zoomControl: enabled });
        }

        if (this.marker) {
            this.marker.setDraggable(enabled);
        }
    }

    public updateValueByInput() {
        const lat = this.geolocationForm.controls['latitude'].value;
        const lng = this.geolocationForm.controls['longitude'].value;

        this.updateValue(lat, lng);

        if (lat && lng) {
            this.updateMarker(true, true);
        } else {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    public ngAfterViewInit() {
        if (!this.isGoogleMaps) {
            this.ngAfterViewInitOSM();
        } else {
            this.ngAfterViewInitGoogle(this.uiOptions.get('map.googleMaps.key'));
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
                        if (!this.marker && !this.snapshot.isDisabled) {
                            const latlng = event.latlng.wrap();

                            this.updateValue(latlng.lat, latlng.lng);
                            this.updateMarker(false, true);
                        }
                    });

                this.updateMarker(true, false);

                if (this.snapshot.isDisabled) {
                    this.map.zoomControl.disable();

                    this.map._handlers.forEach((handler: any) => {
                        handler.disable();
                    });
                }
            });
    }

    private ngAfterViewInitGoogle(apiKey: string) {
        this.resourceLoader.loadScript(`https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places`).then(
            () => {
                this.map = new google.maps.Map(this.editor.nativeElement,
                    {
                        zoom: 1,
                        mapTypeControl: false,
                        streetViewControl: false,
                        center: { lat: 0, lng: 0 }
                    });

                const searchBox = new google.maps.places.SearchBox(this.searchBoxInput.nativeElement);

                this.map.addListener('click',
                    (event: any) => {
                        if (!this.snapshot.isDisabled) {
                            this.updateValue(event.latLng.lat(), event.latLng.lng());
                            this.updateMarker(false, true);
                        }
                    });

                this.map.addListener('bounds_changed', () => {
                    searchBox.setBounds(this.map.getBounds());
                });

                searchBox.addListener('places_changed', () => {
                    let places = searchBox.getPlaces();

                    if (places.length === 1) {
                        let place = places[0];

                        if (!place.geometry) {
                            return;
                        }

                        if (!this.snapshot.isDisabled) {
                            let lat = place.geometry.location.lat();
                            let lng = place.geometry.location.lng();

                            this.updateValue(lat, lng);
                            this.updateMarker(false, true);
                        }
                    }
                });

                this.updateMarker(true, false);

                if (this.snapshot.isDisabled) {
                    this.map.setOptions({ draggable: false, zoomControl: false });
                }
            });
    }

    public reset() {
        this.value = null;
        this.searchBoxInput.nativeElement.value = '';

        this.updateMarker(true, true);
    }

    private updateValue(lat: number, lng: number) {
        this.value = { latitude: lat, longitude: lng };
    }

    private updateMarker(zoom: boolean, fireEvent: boolean) {
        if (!this.snapshot.isGoogleMaps) {
            this.updateMarkerOSM(zoom);
        } else {
            this.updateMarkerGoogle(zoom);
        }

        if (this.value) {
            this.geolocationForm.setValue(this.value, { emitEvent: true, onlySelf: false });
        } else {
            this.geolocationForm.reset(undefined, { emitEvent: true, onlySelf: false });
        }

        if (fireEvent) {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    private updateMarkerOSM(zoom: boolean) {
        if (this.value) {
            if (!this.marker) {
                this.marker = L.marker([0, 90], { draggable: true }).addTo(this.map);

                this.marker.on('drag', (event: any) => {
                    const latlng = event.latlng.wrap();

                    this.updateValue(latlng.lat, latlng.lng);
                });

                this.marker.on('dragend', () => {
                    this.updateMarker(false, true);
                });

                if (this.snapshot.isDisabled) {
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
        } else {
            if (this.marker) {
                this.marker.removeFrom(this.map);
                this.marker = null;
            }

            this.map.fitWorld();
        }
    }

    private updateMarkerGoogle(zoom: boolean) {
        if (this.value) {
            if (!this.marker) {
                this.marker =  new google.maps.Marker({
                    position: {
                        lat: 0,
                        lng: 0
                    },
                    map: this.map,
                    draggable: true
                });

                this.marker.addListener('drag', (event: any) => {
                    if (!this.snapshot.isDisabled) {
                        this.updateValue(event.latLng.lat(), event.LatLng.lng());
                    }
                });
                this.marker.addListener('dragend', (event: any) => {
                    if (!this.snapshot.isDisabled) {
                        this.updateValue(event.latLng.lat(), event.LatLng.lng());
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
            this.map.setZoom(12);
        } else {
            if (this.marker) {
                this.marker.setMap(null);
                this.marker = null;
            }

            this.map.setCenter({ lat: 0, lng: 0 });
        }
    }
}