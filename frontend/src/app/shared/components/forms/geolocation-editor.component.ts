/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, DestroyRef, effect, ElementRef, inject, input, model, signal, untracked, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { disabled, form, FormField, FormValueControl } from '@angular/forms/signals';
import { ControlErrorsComponent, ResizedDirective, SignalValidators, StringHelper, TooltipDirective, TranslatePipe } from '@app/framework';
import { LocalStoreService, ResourceLoaderService, Settings, Types, UIOptions } from '@app/shared/internal';

declare const L: any;
declare const google: any;

type Geolocation = { latitude: number | null; longitude: number | null };

type UpdateOptions = { reset?: boolean; pan?: true; fire?: boolean };

@Component({
    selector: 'sqx-geolocation-editor',
    styleUrls: ['./geolocation-editor.component.scss'],
    templateUrl: './geolocation-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        FormsModule,
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class GeolocationEditorComponent implements FormValueControl<Geolocation | null> {
    private readonly uiOptions = inject(UIOptions);
    private readonly resourceLoader = inject(ResourceLoaderService);

    private readonly googleMapsKey = this.uiOptions.value.map.googleMaps.key;
    private readonly localStore = inject(LocalStoreService);
    private readonly editor = viewChild.required<ElementRef<HTMLElement>>('editor');
    private readonly searchBoxInput = viewChild.required<ElementRef<HTMLInputElement>>('searchBox');
    private readonly geolocationModel = signal({ latitude: NaN, longitude: NaN });
    private marker: any;
    private map: any;
    private current: Geolocation | null = null;

    /** The geolocation value. */
    public readonly value = model<Geolocation | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    protected readonly isGoogleMaps = this.uiOptions.value.map.type !== 'OSM';

    protected readonly isMapHidden = this.localStore.boolean(Settings.Local.HIDE_MAP);

    protected readonly isCompact = signal(false);

    protected readonly geolocationForm = form(this.geolocationModel, path => {
        SignalValidators.between(path.latitude, -90, 90);
        SignalValidators.between(path.longitude, -180, 180);
        disabled(path, () => this.disabled());
    });

    constructor() {
        const destroyRef = inject(DestroyRef);

        effect(() => {
            const value = this.value();

            untracked(() => {
                this.applyValue(value);
            });
        });

        effect(() => {
            const isDisabled = this.disabled();

            if (!this.isGoogleMaps) {
                this.updateOSMDisabled(isDisabled);
            } else {
                this.updateGoogleDisabled(isDisabled);
            }
        });

        afterNextRender(() => {
            if (!this.isGoogleMaps) {
                this.initOSM();
            } else {
                this.initGoogle(this.googleMapsKey);
            }
        });

        destroyRef.onDestroy(() => {
            if (!this.isGoogleMaps) {
                this.map?.remove();
            }

            this.map = null;
            this.marker = null;
        });
    }

    protected hideMap(isMapHidden: boolean) {
        this.isMapHidden.set(isMapHidden);
    }

    private applyValue(obj: any) {
        if (Types.equals(obj, this.current)) {
            return;
        }

        if (Types.isObject(obj) && Types.isNumber(obj.latitude) && Types.isNumber(obj.longitude)) {
            this.current = obj;
        } else {
            this.current = null;
        }

        if (this.marker || (!this.marker && this.map && this.current)) {
            this.updateMarker({ reset: !this.marker });
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

    protected updateValueByInput() {
        const { latitude, longitude } = this.geolocationModel();

        const lat = Number.isNaN(latitude) ? null : latitude;
        const lng = Number.isNaN(longitude) ? null : longitude;

        this.updateValue(lat, lng);

        if (lat && lng) {
            this.updateMarker({ pan: true, fire: true });
        } else {
            this.emitValue();
        }
    }

    private async initOSM() {
        this.searchBoxInput().nativeElement.remove();

        await Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/leaflet/leaflet.css'),
            this.resourceLoader.loadLocalScript('dependencies/leaflet/leaflet.js'),
        ]);

        await Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/leaflet/Control.Geocoder.css'),
            this.resourceLoader.loadLocalScript('dependencies/leaflet/Control.Geocoder.js'),
        ]);

        this.map = L.map(this.editor().nativeElement).fitWorld();

        L.tileLayer('https://{s}.tile.osm.org/{z}/{x}/{y}.png',
        {
            attribution: '&copy; <a href="https://osm.org/copyright">OpenStreetMap</a> contributors',
        }).addTo(this.map);

        L.Control.geocoder({
            defaultMarkGeocode: false,
        })
        .on('markgeocode', (event: any) => {
            const center = event.geocode.center;

            if (!this.disabled()) {
                this.updateValue(center.lat, center.lng);
                this.updateMarker({ reset: true, fire: true });
            }
        })
        .addTo(this.map);

        this.map.on('click', (event: any) => {
            if (!this.disabled()) {
                const latlng = event.latlng.wrap();

                this.updateValue(latlng.lat, latlng.lng);
                this.updateMarker({ fire: true });
            }
        });

        this.updateMarker({ reset: true });

        if (this.disabled()) {
            this.map.zoomControl.disable();

            this.map._handlers.forEach((handler: any) => {
                handler.disable();
            });
        }
    }

    private async initGoogle(key: string) {
        await this.resourceLoader.loadScript(`https://maps.googleapis.com/maps/api/js${StringHelper.buildQuery({ key, libraries: 'places' })}`);

        this.map = new google.maps.Map(this.editor().nativeElement, {
            zoom: 1,
            fullscreenControl: false,
            mapTypeControl: false,
            mapTypeControlOptions: {},
            streetViewControl: false,
            center: { lat: 0, lng: 0 },
        });

        const searchBox = new google.maps.places.SearchBox(this.searchBoxInput().nativeElement);

        this.map.addListener('click', (event: any) => {
            if (!this.disabled()) {
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
            if (places.length !== 1) {
                return;
            }

            const place = places[0];
            if (!place.geometry || this.disabled()) {
                return;
            }

            const lat = place.geometry.location.lat();
            const lng = place.geometry.location.lng();

            this.updateValue(lat, lng);
            this.updateMarker({ pan: true, fire: true });
        });

        this.updateMarker({ reset: true });

        if (this.disabled()) {
            this.map.setOptions({ draggable: false, zoomControl: false });
        }
    }

    protected clearValue() {
        this.current = null;

        this.updateMarker({ fire: true });
    }

    private updateValue(latitude: number | null, longitude: number | null) {
        this.current = { latitude, longitude };
    }

    private updateMarker(opts?: UpdateOptions) {
        if (!this.isGoogleMaps) {
            this.updateMarkerOSM(opts);
        } else {
            this.updateMarkerGoogle(opts);
        }

        if (this.current) {
            this.geolocationModel.set({ latitude: this.current.latitude ?? NaN, longitude: this.current.longitude ?? NaN });
        } else {
            this.geolocationModel.set({ latitude: NaN, longitude: NaN });
            this.geolocationForm().reset();
        }

        if (opts && opts.fire) {
            this.emitValue();
        }
    }

    private updateMarkerOSM(opts?: UpdateOptions) {
        if (!this.current) {
            this.marker?.removeFrom(this.map);
            this.marker = null;
            return;
        }

        if (!this.marker) {
            this.marker = L.marker([0, 90], { draggable: true }).addTo(this.map);

            this.marker.on('dragend', (event: any) => {
                const latlng = event.target.getLatLng().wrap();

                this.updateValue(latlng.lat, latlng.lng);
                this.updateMarker({ fire: true });
            });

            if (this.disabled()) {
                this.marker.dragging.disable();
            }
        }

        const latLng = L.latLng(this.current.latitude, this.current.longitude);

        if (opts && opts.reset) {
            this.map.setView(latLng, 15);
        } else if (opts && opts.pan) {
            this.map.panTo(latLng);
        }

        this.marker.setLatLng(latLng);
    }

    private updateMarkerGoogle(opts?: UpdateOptions) {
        if (!this.current) {
            this.marker?.setMap(null);
            this.marker = null;
            return;
        }

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
                if (!this.disabled()) {
                    const latlng = event.latLng;

                    this.updateValue(latlng.lat(), latlng.lng());
                    this.updateMarker({ fire: true });
                }
            });
        }

        const latLng = new google.maps.LatLng(this.current.latitude, this.current.longitude);

        if (opts && opts.reset) {
            this.map.setZoom(12);
            this.map.setCenter(latLng);
        } else if (opts && opts.pan) {
            this.map.setCenter(latLng);
        }

        this.marker.setPosition(latLng);
    }

    protected setCompact(isCompact: boolean) {
        this.isCompact.set(isCompact);
    }

    private emitValue() {
        this.value.set(this.current);
        this.touched.set(true);
    }
}
