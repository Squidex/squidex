/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, forwardRef, Input, numberAttribute, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldForm, FieldSection, FormHintComponent, LocalStoreService, MarkdownDirective, RootFieldDto, SchemaDto, Settings, StatefulComponent, TypedSimpleChanges } from '@app/shared';
import { ContentFieldComponent } from './content-field.component';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-content-section',
    styleUrls: ['./content-section.component.scss'],
    templateUrl: './content-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        FormHintComponent,
        MarkdownDirective,
        forwardRef(() => ContentFieldComponent),
    ],
})
export class ContentSectionComponent extends StatefulComponent<State> {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input({ transform: booleanAttribute })
    public isCompact?: boolean | null;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true })
    public formSection!: FieldSection<RootFieldDto, FieldForm>;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
        super({ isCollapsed: false });

        this.project(x => x.isCollapsed).subscribe(isCollapsed => {
            if (this.formSection?.separator && this.schema) {
                this.localStore.setBoolean(this.isCollapsedKey(), isCollapsed);
            }
        });
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formSection || changes.schema) {
            if (this.formSection?.separator && this.schema) {
                const isCollapsed = this.localStore.getBoolean(this.isCollapsedKey());

                this.next({ isCollapsed });
            }
        }
    }

    public toggle() {
        this.next(s => ({
            ...s,
            isCollapsed: !s.isCollapsed,
        }));
    }

    public getFieldFormCompare(formState: FieldForm) {
        return this.formCompare?.get(formState.field.name);
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.schema?.id, this.formSection?.separator?.fieldId);
    }
}
