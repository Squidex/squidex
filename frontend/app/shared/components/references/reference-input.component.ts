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
            this.contentsService.getContentsByIds(this.appsState.appName, [obj])
                .subscribe({
                    next: contents => {
                        this.selectContent(contents.items[0]);
                    },
                    error: () => {
                        this.selectContent(undefined);
                    },
                });
        } else {
            this.selectContent(undefined);
        }
    }

    public select(contents: ReadonlyArray<ContentDto>) {
        this.selectContent(contents[0]);
    }

    private selectContent(selectedContent?: ContentDto) {
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
                .map(v => v.formatted || this.localizer.getOrKey('common.noValue'))
                .filter(v => !!v)
                .join(', ');

        return name;
    }
}
