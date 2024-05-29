/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, forwardRef, inject, Input, ViewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { ControlErrorsComponent, ResizedDirective, StringHelper, TooltipDirective, TranslatePipe } from '@app/framework';
import { ExtendedFormGroup, LocalStoreService, ResourceLoaderService, Settings, StatefulControlComponent, Types, UIOptions, ValidatorsEx } from '@app/shared/internal';

declare const L: any;
declare const google: any;

export const SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => GeolocationEditorComponent), multi: true,
};

type Geolocation = { latitude: number; longitude: number };

interface State {
    // True when the map should be hidden.
    isMapHidden: boolean;

    // True, when width less than 600 pixels.
    isCompact?: boolean;
}

type UpdateOptions = { reset?: boolean; pan?: true; fire?: boolean };

@Component({
    standalone: true,
    selector: 'sqx-geolocation-editor',
    styleUrls: ['./geolocation-editor.component.scss'],
    templateUrl: './geolocation-editor.component.html',
    providers: [
        SQX_GEOLOCATION_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class GeolocationEditorComponent extends StatefulControlComponent<State, Geolocation> implements AfterViewInit {
    private readonly googleMapsKey = inject(UIOptions).value.map.googleMaps.key;
    private marker: any;
    private map: any;
    private value: Geolocation | null = null;

    @ViewChild('editor', { static: false })
    public editor!: ElementRef<HTMLElement>;

    @ViewChild('searchBox', { static: false })
    public searchBoxInput!: ElementRef<HTMLInputElement>;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public readonly isGoogleMaps = inject(UIOptions).value.map.type !== 'OSM';

    public geolocationForm =
        new ExtendedFormGroup({
            latitude: new UntypedFormControl('',
                ValidatorsEx.between(-90, 90),
            ),
            longitude: new UntypedFormControl('',
                ValidatorsEx.between(-180, 180),
            ),
        });

    public get hasValue() {
        return !!this.value;
    }

    constructor(localStore: LocalStoreService,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super({ isMapHidden: localStore.getBoolean(Settings.Local.HIDE_MAP) });

        this.project(x => x.isMapHidden).subscribe(isMapHidden => {
            localStore.setBoolean(Settings.Local.HIDE_MAP, isMapHidden);
        });
    }

    public hideMap(isMapHidden: boolean) {
        this.next({ isMapHidden });
    }

    public writeValue(obj: any) {
        if (Types.isObject(obj) && Types.isNumber(obj.latitude) && Types.isNumber(obj.longitude)) {
            this.value = obj;
        } else {
            this.value = null;
        }

        if (this.marker || (!this.marker && this.map && this.value)) {
            this.updateMarker({ reset: !this.marker });
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (!this.isGoogleMaps) {
            this.updateOSMDisabled(isDisabled);
        } else {
            this.updateGoogleDisabled(isDisabled);
        }

        if (isDisabled) {
            this.geolocationForm.disable();
        } else {
            this.geolocationForm.enable();
        }
    }

    private updateOSMDisabled(isDisabled: boolean) {
        const update: (t: any) => any =
            isDisabled ?
            x => x.disable() :
            x => x.enable();

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

    private updateGoogleDisabled(isDisabled: boolean) {
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
            this.updateMarker({ pan: true, fire: true });
        } else {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    public ngAfterViewInit() {
        if (!this.isGoogleMaps) {
            this.ngAfterViewInitOSM();
        } else {
            this.ngAfterViewInitGoogle(this.googleMapsKey);
        }
    }

    private async ngAfterViewInitOSM() {
        this.searchBoxInput.nativeElement.remove();

        await Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/leaflet/leaflet.css'),
            this.resourceLoader.loadLocalScript('dependencies/leaflet/leaflet.js'),
        ]);

        await Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/leaflet/Control.Geocoder.css'),
            this.resourceLoader.loadLocalScript('dependencies/leaflet/Control.Geocoder.min.js'),
        ]);

        this.map = L.map(this.editor.nativeElement).fitWorld();

        L.tileLayer('https://{s}.tile.osm.org/{z}/{x}/{y}.png',
        {
            attribution: '&copy; <a href="https://osm.org/copyright">OpenStreetMap</a> contributors',
        }).addTo(this.map);

        L.Control.geocoder({
            defaultMarkGeocode: false,
        })
        .on('markgeocode', (event: any) => {
            const center = event.geocode.center;

            if (!this.snapshot.isDisabled) {
                this.updateValue(center.lat, center.lng);
                this.updateMarker({ reset: true, fire: true });
            }
        })
        .addTo(this.map);

        this.map.on('click', (event: any) => {
            if (!this.snapshot.isDisabled) {
                const latlng = event.latlng.wrap();

                this.updateValue(latlng.lat, latlng.lng);
                this.updateMarker({ fire: true });
            }
        });

        this.updateMarker({ reset: true });

        if (this.snapshot.isDisabled) {
            this.map.zoomControl.disable();

            this.map._handlers.forEach((handler: any) => {
                handler.disable();
            });
        }
    }

    private async ngAfterViewInitGoogle(key: string) {
        await this.resourceLoader.loadScript(`https://maps.googleapis.com/maps/api/js${StringHelper.buildQuery({ key, libraries: 'places' })}`);

        this.map = new google.maps.Map(this.editor.nativeElement, {
            zoom: 1,
            fullscreenControl: false,
            mapTypeControl: false,
            mapTypeControlOptions: {},
            streetViewControl: false,
            center: { lat: 0, lng: 0 },
        });

        const searchBox = new google.maps.places.SearchBox(this.searchBoxInput.nativeElement);

        this.map.addListener('click', (event: any) => {
            if (!this.snapshot.isDisabled) {
                const latlng = event.latLng;

                this.updateValue(latlng.lat(), latlng.lng());
                this.updateMarker({ fire: true });
            }
        });

        this.map.addListener('bounds_changed', () => {
            searchBox.setBounds(this.map.getBounds());
        });

        searchBox.addListener('places_changed', () => {
            const places = searchBox.getPlaces();

            if (places.length === 1) {
                const place = places[0];

                if (!place.geometry) {
                    return;
                }

                if (!this.snapshot.isDisabled) {
                    const lat = place.geometry.location.lat();
                    const lng = place.geometry.location.lng();

                    this.updateValue(lat, lng);
                    this.updateMarker({ pan: true, fire: true });
                }
            }
        });

        this.updateMarker({ reset: true });

        if (this.snapshot.isDisabled) {
            this.map.setOptions({ draggable: false, zoomControl: false });
        }
    }

    public clearValue() {
        this.value = null;

        this.updateMarker({ fire: true });
    }

    private updateValue(latitude: number, longitude: number) {
        this.value = { latitude, longitude };

        this.detectChanges();
    }

    private updateMarker(opts?: UpdateOptions) {
        if (!this.isGoogleMaps) {
            this.updateMarkerOSM(opts);
        } else {
            this.updateMarkerGoogle(opts);
        }

        if (this.value) {
            this.geolocationForm.setValue(this.value, { emitEvent: true, onlySelf: false });
        } else {
            this.geolocationForm.reset(undefined, { emitEvent: true, onlySelf: false });
        }

        if (opts && opts.fire) {
            this.callChange(this.value);
            this.callTouched();
        }
    }

    private updateMarkerOSM(opts?: UpdateOptions) {
        if (this.value) {
            if (!this.marker) {
                this.marker = L.marker([0, 90], { draggable: true }).addTo(this.map);

                this.marker.on('dragend', (event: any) => {
                    const latlng = event.target.getLatLng().wrap();

                    this.updateValue(latlng.lat, latlng.lng);
                    this.updateMarker({ fire: true });
                });

                if (this.snapshot.isDisabled) {
                    this.marker.dragging.disable();
                }
            }

            const latLng = L.latLng(this.value.latitude, this.value.longitude);

            if (opts && opts.reset) {
                this.map.setView(latLng, 15);
            } else if (opts && opts.pan) {
                this.map.panTo(latLng);
            }

            this.marker.setLatLng(latLng);
        } else if (this.marker) {
            this.marker.removeFrom(this.map);
            this.marker = null;
        }
    }

    private updateMarkerGoogle(opts?: UpdateOptions) {
        if (this.value) {
            if (!this.marker) {
                this.marker = new google.maps.Marker({
                    map: this.map,
                    position: {
                        lat: 0,
                        lng: 0,
                    },
                    draggable: true,
                });

                this.marker.addListener('dragend', (event: any) => {
                    if (!this.snapshot.isDisabled) {
                        const latlng = event.latLng;

                        this.updateValue(latlng.lat(), latlng.lng());
                        this.updateMarker({ fire: true });
                    }
                });
            }

            const latLng = new google.maps.LatLng(this.value.latitude, this.value.longitude);

            if (opts && opts.reset) {
                this.map.setZoom(12);
                this.map.setCenter(latLng);
            } else if (opts && opts.pan) {
                this.map.setCenter(latLng);
            }

            this.marker.setPosition(latLng);
        } else if (this.marker) {
            this.marker.setMap(null);
            this.marker = null;
        }
    }

    public setCompact(isCompact: boolean) {
        this.next({ isCompact });
    }
}
