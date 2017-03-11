/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, ElementRef, forwardRef, ViewChild } from '@angular/core';
import { ControlValueAccessor, FormBuilder, NG_VALUE_ACCESSOR } from '@angular/forms';

import { ResourceLoaderService } from './../services/resource-loader.service';
import { ValidatorsEx } from './validators';

const NOOP = () => { /* NOOP */ };

declare var L: any;

export const SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GeolocationEditorComponent), multi: true
};

@Component({
    selector: 'sqx-geolocation-editor',
    styleUrls: ['./geolocation-editor.component.scss'],
    templateUrl: './geolocation-editor.component.html',
    providers: [SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class GeolocationEditorComponent implements ControlValueAccessor, AfterViewInit {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;
    private marker: any;
    private map: any;
    private value: any;

    public get hasValue() {
        return !!this.value;
    }

    public geolocationForm =
        this.formBuilder.group({
            latitude: ['',
                [
                    ValidatorsEx.between(-90, 90)
                ]],
            longitude: ['',
                [
                    ValidatorsEx.between(-180, 180)
                ]]
        });

    public isDisabled = false;

    @ViewChild('editor')
    public editor: ElementRef;

    constructor(
        private readonly resourceLoader: ResourceLoaderService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public writeValue(value: any) {
        this.value = value;

        if (this.marker) {
            this.updateMarker(true, false);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public updateValueByInput() {
        if (this.geolocationForm.controls['latitude'].value !== null &&
            this.geolocationForm.controls['longitude'].value !== null &&
            this.geolocationForm.valid) {
            this.value = this.geolocationForm.value;
        } else {
            this.value = null;
        }

        this.updateMarker(true, true);
    }

    public ngAfterViewInit() {
        this.resourceLoader.loadStyle('https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.0.3/leaflet.css');
        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.0.3/leaflet.js').then(() => {
            this.map = L.map(this.editor.nativeElement).fitWorld();

            L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(this.map);

            this.map.on('click', (event: any) => {
                this.value = { latitude: event.latlng.lat, longitude: event.latlng.lng };

                this.updateMarker(false, true);
            });

            this.updateMarker(true, false);
        });
    }

    public reset() {
        this.value = null;

        this.updateMarker(true, true);
    }

    private updateMarker(zoom: boolean, fireEvent: boolean) {
        if (this.value) {
            if (!this.marker) {
                this.marker = L.marker([0, 90], { draggable: true }).addTo(this.map);

                this.marker.on('drag', (event: any) => {
                    this.value = { latitude: event.latlng.lat, longitude: event.latlng.lng };
                });

                this.marker.on('dragend', (event: any) => {
                    this.updateMarker(false, true);
                });
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
            this.changeCallback(this.value);
            this.touchedCallback();
        }
    }
}