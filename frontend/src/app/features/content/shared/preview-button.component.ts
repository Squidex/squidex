/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { AuthService, ContentDto, DropdownMenuComponent, interpolate, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, Settings, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-preview-button',
    styleUrls: ['./preview-button.component.scss'],
    templateUrl: './preview-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class PreviewButtonComponent {
    private readonly authService = inject(AuthService);
    private readonly localStore = inject(LocalStoreService);

    /** Asks the user for confirmation before the preview is opened, for example to save pending changes. */
    public readonly confirm = input<() => Observable<boolean>>();

    /** The content to preview. */
    public readonly content = input.required<ContentDto>();

    /** The schema of the content, with the preview URLs. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly dropdown = new ModalModel();

    protected readonly previewNameSelected = linkedSignal<string | undefined>(() => {
        const previewUrls = this.schema().previewUrls;

        const selectedName = this.localStore.get(this.configKey());
        if (!selectedName || !previewUrls[selectedName]) {
            return Object.keys(previewUrls)[0];
        }

        return selectedName;
    });

    protected readonly previewNamesMore = computed(() => {
        const previewNameSelected = this.previewNameSelected();
        const previewNames = Object.keys(this.schema().previewUrls);
        if (!previewNameSelected) {
            return previewNames.sorted();
        }

        return previewNames.removed(previewNameSelected).sorted();
    });

    protected follow(name?: string) {
        if (name) {
            this.selectUrl(name);

            const confirm = this.confirm();
            if (confirm) {
                confirm().pipe(take(1))
                    .subscribe(confirmed => {
                        if (confirmed) {
                            this.navigateTo(name);
                        }
                    });
            } else {
                this.navigateTo(name);
            }
        }

        this.dropdown.hide();
    }

    private navigateTo(name: string) {
        const urlVars = { ...this.content(), ...this.authService.user || {} };
        const urlLink = interpolate(this.schema().previewUrls[name], urlVars, 'iv');

        window.open(urlLink, '_blank');
    }

    private selectUrl(selectedName: string) {
        this.previewNameSelected.set(selectedName);

        this.localStore.set(this.configKey(), selectedName);
    }

    private configKey() {
        return Settings.Local.SCHEMA_PREVIEW(this.schema().id);
    }
}
