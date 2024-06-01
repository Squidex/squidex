/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { AuthService, ContentDto, DropdownMenuComponent, interpolate, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, Settings, StatefulComponent, TranslatePipe } from '@app/shared';

interface State {
    // The name of the selected preview config.
    previewNameSelected?: string;

    // All other preview configs.
    previewNamesMore: ReadonlyArray<string>;
}

@Component({
    standalone: true,
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
export class PreviewButtonComponent extends StatefulComponent<State> implements OnInit {
    @Input()
    public confirm?: () => Observable<boolean>;

    @Input({ required: true })
    public content!: ContentDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    public dropdown = new ModalModel();

    constructor(
        private readonly authService: AuthService,
        private readonly localStore: LocalStoreService,
    ) {
        super({ previewNamesMore: [] });
    }

    public ngOnInit() {
        let selectedName = this.localStore.get(this.configKey());

        if (!selectedName || !this.schema.previewUrls[selectedName]) {
            selectedName = Object.keys(this.schema.previewUrls)[0];
        }

        this.selectUrl(selectedName);
    }

    public follow(name?: string) {
        if (name) {
            this.selectUrl(name);

            if (this.confirm) {
                this.confirm().pipe(take(1))
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
        const vars = { ...this.content, ...this.authService.user || {} };

        const url = interpolate(this.schema.previewUrls[name], vars, 'iv');

        window.open(url, '_blank');
    }

    private selectUrl(selectedName: string) {
        this.next(s => {
            if (selectedName === s.previewNameSelected) {
                return s;
            }

            const state = { ...s };

            const keys = Object.keys(this.schema.previewUrls);

            state.previewNameSelected = selectedName;
            state.previewNamesMore = keys.removed(selectedName).sorted();

            this.localStore.set(this.configKey(), selectedName);

            return state;
        });
    }

    private configKey() {
        return Settings.Local.SCHEMA_PREVIEW(this.schema.id);
    }
}
