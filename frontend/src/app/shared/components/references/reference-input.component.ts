/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ModalDirective, SignalControlComponent } from '@app/framework';
import { AppLanguageDto, AppsState, ContentDto, ContentsService, DialogModel, getContentValue, LocalizerService, Types } from '@app/shared/internal';
import { ContentSelectorComponent } from './content-selector.component';

export const SQX_REFERENCE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferenceInputComponent), multi: true,
};

@Component({
    selector: 'sqx-reference-input',
    styleUrls: ['./reference-input.component.scss'],
    templateUrl: './reference-input.component.html',
    providers: [
        SQX_REFERENCE_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentSelectorComponent,
        FormsModule,
        ModalDirective,
    ],
})
export class ReferenceInputComponent extends SignalControlComponent<ReadonlyArray<string> | string> {
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

    public readonly contentSelectorDialog = new DialogModel();

    public readonly selectedContent = signal<ContentDto | undefined>(undefined);

    public readonly selectedName = computed(() => {
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

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super();
    }

    public writeValue(obj: any) {
        if (Types.isString(obj)) {
            this.loadContent(obj);
        } else if (Types.isArrayOfString(obj)) {
            this.loadContent(obj[0]);
        } else {
            this.selectedContent.set(undefined);
        }
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

    public openDialog() {
        if (this.isDisabled()) {
            return;
        }

        this.contentSelectorDialog.show();
    }

    public select(contents: ReadonlyArray<ContentDto>, clear = false) {
        if (contents.length > 0 || clear) {
            this.selectContent(contents[0]);
        }

        this.contentSelectorDialog.hide();
    }

    public selectContent(selectedContent?: ContentDto) {
        if (this.isDisabled()) {
            return;
        }

        const id = selectedContent?.id;
        const isSingle = this.mode() === 'Single';

        if (id) {
            this.callChange(isSingle ? id : [id]);
        } else {
            this.callChange(isSingle ? null : []);
        }

        this.callTouched();
        this.selectedContent.set(selectedContent);
    }
}
