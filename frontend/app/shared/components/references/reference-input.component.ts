/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppsState, ContentDto, ContentsService, DialogModel, getContentValue, LanguageDto, LocalizerService, StatefulControlComponent, Types } from '@app/shared/internal';

export const SQX_REFERENCE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferenceInputComponent), multi: true,
};

interface State {
    // The referenced content item.
    selectedContent?: ContentDto;

    // The name of the selected item.
    selectedName?: string;
}

@Component({
    selector: 'sqx-reference-input[language][languages]',
    styleUrls: ['./reference-input.component.scss'],
    templateUrl: './reference-input.component.html',
    providers: [
        SQX_REFERENCE_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferenceInputComponent extends StatefulControlComponent<State, string> implements OnChanges {
    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public contentSelectorDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly localizer: LocalizerService,
    ) {
        super(changeDetector, {});
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['language']) {
            this.selectContent(this.snapshot.selectedContent);
        }
    }

    public writeValue(obj: any) {
        if (Types.isString(obj)) {
            this.contentsService.getAllContents(this.appsState.appName, { ids: [obj] })
                .subscribe({
                    next: contents => {
                        this.updateContent(contents.items[0]);
                    },
                    error: () => {
                        this.updateContent(undefined);
                    },
                });
        } else {
            this.updateContent();
        }
    }

    public select(contents: ReadonlyArray<ContentDto>) {
        if (contents.length > 0) {
            this.selectContent(contents[0]);
        }

        this.contentSelectorDialog.hide();
    }

    public selectContent(selectedContent?: ContentDto) {
        this.callChange(selectedContent?.id);
        this.callTouched();

        this.updateContent(selectedContent);
    }

    private updateContent(selectedContent?: ContentDto) {
        this.next(s => ({
            ...s,
            selectedContent,
            selectedName: this.createContentName(selectedContent),
        }));
    }

    private createContentName(content?: ContentDto) {
        if (!content) {
            return undefined;
        }

        const name =
            content.referenceFields
                .map(f => getContentValue(content, this.language, f, false))
                .map(v => v.formatted)
                .filter(v => !!v)
                .join(', ')
            || this.localizer.getOrKey('common.noValue');

        return name || this.localizer.getOrKey('common.noValue');
    }
}
