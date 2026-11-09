/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, linkedSignal, model, signal } from '@angular/core';
import { of, timer } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { DialogModel, ExternalLinkDirective, FocusOnInitDirective, FormValueControlBase, LoaderComponent, ModalDialogComponent, ModalDirective, safeResource, StockPhotoDto, StockPhotoService, StopClickDirective, thumbnail, TooltipDirective, TranslatePipe } from '@app/shared';

type StockPhotoPage = { page: number; items: ReadonlyArray<StockPhotoDto> };

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
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
    private readonly stockPhotoService = inject(StockPhotoService);

    /** The value of the control. */
    public readonly value = model<string | null>(null);

    protected readonly stockPhotoSearch = signal('');

    protected readonly stockPhotoPage = linkedSignal(() => {
        this.stockPhotoSearch();

        return 1;
    });

    protected readonly stockPhotos = linkedSignal<StockPhotoPage | undefined, ReadonlyArray<StockPhotoDto>>({
        source: () => this.stockPhotoResult.isLoading() ? undefined : this.stockPhotoResult.value(),
        computation: (result, previous) => {
            // Keep the current photos while the next page or search is loading.
            if (!result) {
                return previous?.value ?? [];
            }

            if (result.page > 1) {
                return [...previous?.value ?? [], ...result.items];
            }

            return result.items;
        },
    });

    protected readonly stockPhotosHasMore = linkedSignal<StockPhotoPage | undefined, boolean>({
        source: () => this.stockPhotoResult.isLoading() ? undefined : this.stockPhotoResult.value(),
        computation: (result, previous) => {
            if (!result) {
                return previous?.value ?? false;
            }

            return result.items.length === 20;
        },
    });

    protected readonly thumbnailStatus = linkedSignal<string | null, 'Loaded' | 'Failed' | undefined>({
        source: this.value,
        computation: () => undefined,
    });

    protected readonly stockPhotosLoading = computed(() => {
        return this.stockPhotoResult.isLoading() && this.stockPhotoSearch().length > 0;
    });

    protected readonly stockPhotoThumbnail = computed(() => {
        const value = this.value();

        return value ? thumbnail(value, undefined, 300) || value : null;
    });

    protected readonly stockPhotoResult = safeResource<StockPhotoPage, { search: string; page: number }>({
        params: () => ({ search: this.stockPhotoSearch(), page: this.stockPhotoPage() }),
        stream: ({ params: { search, page } }) => {
            if (!search) {
                return of({ page, items: [] });
            }

            // The previous request is cancelled when the params change, so the timer debounces typing.
            return timer(page > 1 ? 0 : 500).pipe(
                switchMap(() => this.stockPhotoService.getImages(search, page)),
                map(items => ({ page, items })));
        },
        defaultValue: { page: 1, items: [] },
    });

    protected readonly searchDialog = new DialogModel();

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
        this.stockPhotoPage.update(x => x + 1);
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
