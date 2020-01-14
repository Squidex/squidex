/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { of } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, shareReplay, startWith, switchMap, tap } from 'rxjs/operators';

import {
    StatefulControlComponent,
    StockPhotoDto,
    StockPhotoService,
    thumbnail,
    Types
} from '@app/shared';

interface State {
    isLoading?: boolean;
}

export const SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StockPhotoEditorComponent), multi: true
};

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    providers: [SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StockPhotoEditorComponent extends StatefulControlComponent<State, string> implements OnInit {
    @Input()
    public isCompact = false;

    public valueControl = new FormControl('');

    public valueThumb =
        this.valueControl.valueChanges.pipe(
            startWith(this.valueControl.value),
            shareReplay(1),
            map(value => thumbnail(value, 400) || value));

    public stockPhotoSearch = new FormControl('');

    public stockPhotos =
        this.stockPhotoSearch.valueChanges.pipe(
            startWith(this.stockPhotoSearch.value),
            distinctUntilChanged(),
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
        this.own(this.valueThumb);

        this.own(
            this.valueControl.valueChanges
                .subscribe(value => {
                    this.callChange(value);
                }));
    }

    public writeValue(obj: string) {
        if (Types.isString(obj)) {
            this.valueControl.setValue(obj, { emitEvent: true });
        } else {
            this.valueControl.setValue('', { emitEvent: true });
        }
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