/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, linkedSignal, model, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, of } from 'rxjs';
import { debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { DialogModel, ExternalLinkDirective, FocusOnInitDirective, FormValueControlBase, LoaderComponent, ModalDialogComponent, ModalDirective, StockPhotoDto, StockPhotoService, StopClickDirective, thumbnail, TooltipDirective, TranslatePipe } from '@app/shared';

type Request = { search?: string; page: number };

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        FocusOnInitDirective,
        LoaderComponent,
        ModalDialogComponent,
        ModalDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class StockPhotoEditorComponent extends FormValueControlBase<string | null> {
    private readonly stockPhotoRequests = new BehaviorSubject<Request>({ page: 1 });
    private readonly stockPhotoService = inject(StockPhotoService);

    /** The value of the control. */
    public readonly value = model<string | null>(null);

    protected readonly stockPhotoSearch = signal('');

    protected readonly stockPhotos = signal<ReadonlyArray<StockPhotoDto>>([]);

    protected readonly stockPhotosHasMore = signal(false);

    protected readonly stockPhotosLoading = signal(false);

    protected readonly thumbnailStatus = linkedSignal<string | null, 'Loaded' | 'Failed' | undefined>({
        source: this.value,
        computation: () => undefined,
    });

    protected readonly stockPhotoThumbnail = computed(() => {
        const value = this.value();

        return value ? thumbnail(value, undefined, 300) || value : null;
    });

    protected readonly searchDialog = new DialogModel();

    constructor() {
        super();

        effect(() => {
            this.stockPhotoRequests.next({ search: this.stockPhotoSearch(), page: 1 });
        });

        this.stockPhotoRequests.pipe(
                debounceTime(500),
                tap(request => {
                    if (request.search && request.search.length > 0) {
                        this.stockPhotosLoading.set(true);
                    }
                }),
                switchMap(request => {
                    if (request.search && request.search.length > 0) {
                        return this.stockPhotoService.getImages(request.search, request.page).pipe(map(result => ({ request, result })));
                    } else {
                        return of(({ request, result: [] }));
                    }
                }),
                takeUntilDestroyed())
            .subscribe(({ request, result }) => {
                this.stockPhotosLoading.set(false);
                this.stockPhotos.update(x => request.page > 1 ? [...x, ...result] : result);
                this.stockPhotosHasMore.set(result.length === 20);
            });
    }

    protected selectPhoto(photo: StockPhotoDto) {
        if (!this.disabled()) {
            this.updateValue(photo.url);

            this.searchDialog.hide();
        }
    }

    public reset() {
        if (!this.disabled()) {
            this.updateValue(null);
        }
    }

    protected search(event: Event) {
        this.stockPhotoSearch.set((event.target as HTMLInputElement).value);
    }

    protected loadMore() {
        const request = this.stockPhotoRequests.value;

        this.stockPhotoRequests.next({ search: request.search, page: request.page + 1 });
    }

    protected onThumbnailLoaded() {
        this.thumbnailStatus.set('Loaded');
    }

    protected onThumbnailFailed() {
        this.thumbnailStatus.set('Failed');
    }

    protected isSelected(photo: StockPhotoDto) {
        return photo.url === this.value();
    }

    private updateValue(value: string | null) {
        this.value.set(value);
        this.markTouched();
    }
}
