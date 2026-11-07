/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, inject, input, model, signal } from '@angular/core';
import { FormValueControlBase, ModalDirective, syncExternalValue } from '@app/framework';
import { AppLanguageDto, AppsState, ContentDto, ContentsService, DialogModel, getContentValue, LocalizerService, Types } from '@app/shared/internal';
import { ContentSelectorComponent } from './content-selector.component';

@Component({
    selector: 'sqx-reference-input',
    styleUrls: ['./reference-input.component.scss'],
    templateUrl: './reference-input.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentSelectorComponent,
        ModalDirective,
    ],
})
export class ReferenceInputComponent extends FormValueControlBase<ReadonlyArray<string> | string | null> {
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly localizer = inject(LocalizerService);
    private readonly valueSetter = syncExternalValue(() => this.value, value => this.applyValue(value));

    /** The selected content ID, or an array with the selected content ID. */
    public readonly value = model<ReadonlyArray<string> | string | null>(null);

    /** The IDs or names of the schemas to select the content from. */
    public readonly schemaIds = input<ReadonlyArray<string>>();

    /** The initial full text query of the content selector. */
    public readonly query = input.required<string | undefined>();

    /** The language to build the name of the selected content. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Defines whether the value is a single ID or an array with one ID. */
    public readonly mode = input.required<'Array' | 'Single'>();

    /** Renders a small input. */
    public readonly small = input(false, { transform: booleanAttribute });

    protected readonly selectedContent = signal<ContentDto | undefined>(undefined);

    protected readonly selectedName = computed(() => {
        const content = this.selectedContent();
        if (!content) {
            return undefined;
        }

        const language = this.language();
        const name =
            content.referenceFields
                .map(f => getContentValue(content, language, f, false))
                .map(v => v.formatted)
                .defined()
                .join(', ');

        return name || this.localizer.getOrKey('common.noValue');
    });

    protected readonly contentSelectorDialog = new DialogModel();

    private applyValue(value: ReadonlyArray<string> | string | null) {
        let id: string | undefined;

        if (Types.isString(value)) {
            id = value;
        } else if (Types.isArrayOfString(value)) {
            id = value[0];
        }

        if (!id) {
            this.selectedContent.set(undefined);
            return;
        }

        if (id === this.selectedContent()?.id) {
            return;
        }

        this.loadContent(id);
    }

    private loadContent(id: string) {
        this.contentsService.getAllContents(this.appsState.appName, { ids: [id] })
            .subscribe({
                next: contents => {
                    this.selectedContent.set(contents.items[0]);
                },
                error: () => {
                    this.selectedContent.set(undefined);
                },
            });
    }

    protected openDialog() {
        if (this.disabled()) {
            return;
        }

        this.contentSelectorDialog.show();
    }

    protected select(contents: ReadonlyArray<ContentDto>, clear = false) {
        if (contents.length > 0 || clear) {
            this.selectContent(contents[0]);
        }

        this.contentSelectorDialog.hide();
    }

    private selectContent(selectedContent?: ContentDto) {
        if (this.disabled()) {
            return;
        }

        const id = selectedContent?.id;
        const isSingle = this.mode() === 'Single';

        if (id) {
            this.valueSetter(isSingle ? id : [id]);
        } else {
            this.valueSetter(isSingle ? null : []);
        }

        this.markTouched();
        this.selectedContent.set(selectedContent);
    }
}
