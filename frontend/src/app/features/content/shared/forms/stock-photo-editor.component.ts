/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { BehaviorSubject, of } from 'rxjs';
import { debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { DialogModel, ExternalLinkDirective, FocusOnInitDirective, LoaderComponent, ModalDialogComponent, ModalDirective, StatefulControlComponent, StockPhotoDto, StockPhotoService, StopClickDirective, Subscriptions, thumbnail, TooltipDirective, TranslatePipe, Types, value$, valueProjection$ } from '@app/shared';

export const SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StockPhotoEditorComponent), multi: true,
};

interface State {
    // True when loading assets.
    isLoading?: boolean;

    // The photos.
    stockPhotos: ReadonlyArray<StockPhotoDto>;

    // True if more photos are available.
    hasMore?: boolean;

    // The status of the thumbnail.
    thumbnailStatus?: 'Loaded' | 'Failed';
}

type Request = { search?: string; page: number };

@Component({
    standalone: true,
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    providers: [
        SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ExternalLinkDirective,
        FocusOnInitDirective,
        FormsModule,
        LoaderComponent,
        ModalDialogComponent,
        ModalDirective,
        ReactiveFormsModule,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class StockPhotoEditorComponent extends StatefulControlComponent<State, string> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public valueControl = new UntypedFormControl('');

    public stockPhotoRequests = new BehaviorSubject<Request>({ page: 1 });
    public stockPhotoThumbnail = valueProjection$(this.valueControl, x => thumbnail(x, undefined, 300) || x);
    public stockPhotoSearch = new UntypedFormControl('');

    public searchDialog = new DialogModel();

    constructor(
        private readonly stockPhotoService: StockPhotoService,
    ) {
        super({ stockPhotos: [] });
    }

    public ngOnInit() {
        this.subscriptions.add(
            value$(this.valueControl)
                .subscribe(() => {
                    this.next({ thumbnailStatus: undefined });
                }));

        this.subscriptions.add(
            value$(this.stockPhotoSearch)
                .subscribe(search => {
                    this.stockPhotoRequests.next({ search, page: 1 });
                }));

        this.subscriptions.add(
            this.valueControl.valueChanges
                .subscribe(value => {
                    this.callChange(value);
                    this.callTouched();
                }));

        this.subscriptions.add(
            this.stockPhotoRequests.pipe(
                debounceTime(500),
                tap(request => {
                    if (request.search && request.search.length > 0) {
                        this.next({ isLoading: true });
                    }
                }),
                switchMap(request => {
                    if (request.search && request.search.length > 0) {
                        return this.stockPhotoService.getImages(request.search, request.page).pipe(map(result => ({ request, result })));
                    } else {
                        return of(({ request, result: [] }));
                    }
                }),
                tap(({ request, result }) => {
                    this.next(s => ({
                        ...s,
                        isLoading: false,
                        isDisabled: s.isDisabled,
                        stockPhotos: request.page > 1 ? [...s.stockPhotos, ...result] : result,
                        hasMore: result.length === 20,
                    }));
                })));
    }

    public writeValue(obj: string) {
        if (Types.isString(obj)) {
            this.valueControl.setValue(obj);
        } else {
            this.valueControl.setValue('');
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.stockPhotoSearch.disable({ emitEvent: false });
        } else {
            this.stockPhotoSearch.enable({ emitEvent: false });
        }
    }

    public selectPhoto(photo: StockPhotoDto) {
        if (!this.snapshot.isDisabled) {
            this.valueControl.setValue(photo.url);

            this.searchDialog.hide();
        }
    }

    public reset() {
        if (!this.snapshot.isDisabled) {
            this.valueControl.setValue('');
        }
    }

    public loadMore() {
        const request = this.stockPhotoRequests.value;

        this.stockPhotoRequests.next({ search: request.search, page: request.page + 1 });
    }

    public onThumbnailLoaded() {
        this.next({ thumbnailStatus: 'Loaded' });
    }

    public onThumbnailFailed() {
        this.next({ thumbnailStatus: 'Failed' });
    }

    public isSelected(photo: StockPhotoDto) {
        return photo.url === this.valueControl.value;
    }
}
