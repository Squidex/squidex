/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { of } from 'rxjs';
import { debounceTime, map, switchMap, tap } from 'rxjs/operators';

import {
    StatefulControlComponent,
    StockPhotoDto,
    StockPhotoService,
    thumbnail,
    Types,
    value$
} from '@app/shared';

interface State {
    // True when loading assets.
    isLoading?: boolean;

    // True, when width less than 600 pixels.
    isCompact?: boolean;
}

export const SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StockPhotoEditorComponent), multi: true
};

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    providers: [
        SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StockPhotoEditorComponent extends StatefulControlComponent<State, string> implements OnInit {
    public valueControl = new FormControl('');

    public stockPhotoThumbnail = value$(this.valueControl).pipe(map(v => thumbnail(v, 400) || v));
    public stockPhotoSearch = new FormControl('');

    public stockPhotos =
        value$(this.stockPhotoSearch).pipe(
            debounceTime(500),
            tap(query => {
                if (query && query.length > 0) {
                    this.next({ isLoading: true });
                }
            }),
            switchMap(query => {
                if (query && query.length > 0) {
                    return this.stockPhotoService.getImages(query);
                } else {
                    return of([]);
                }
            }),
            tap(() => {
                this.next({ isLoading: false });
            }));

    constructor(changeDetector: ChangeDetectorRef,
        private readonly stockPhotoService: StockPhotoService
    ) {
        super(changeDetector, {});
    }

    public ngOnInit() {
        this.own(
            this.valueControl.valueChanges
                .subscribe(value => {
                    this.callChange(value);
                }));
    }

    public writeValue(obj: string) {
        if (Types.isString(obj)) {
            this.valueControl.setValue(obj);
        } else {
            this.valueControl.setValue('');
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (isDisabled) {
            this.stockPhotoSearch.disable(NO_EMIT);
        } else {
            this.stockPhotoSearch.enable(NO_EMIT);
        }
    }

    public setCompact(isCompact: boolean) {
        this.next(s => ({ ...s, isCompact: isCompact }));
    }

    public selectPhoto(photo: StockPhotoDto) {
        if (!this.snapshot.isDisabled) {
            this.valueControl.setValue(photo.url);
        }
    }

    public reset() {
        if (!this.snapshot.isDisabled) {
            this.valueControl.setValue('');
        }
    }

    public isSelected(photo: StockPhotoDto) {
        return photo.url === this.valueControl.value;
    }

    public trackByPhoto(index: number, photo: StockPhotoDto) {
        return photo.thumbUrl;
    }
}