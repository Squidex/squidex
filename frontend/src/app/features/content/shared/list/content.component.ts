
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output, QueryList, ViewChildren } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppLanguageDto, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListCellResizeDirective, ContentListFieldComponent, ContentsState, ContentStatusComponent, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, PatchContentForm, SchemaDto, StopClickDirective, TableField, TableSettings, TabRouterlinkDirective, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: '[sqxContent][language][languages][tableFields][schema][tableSettings]',
    styleUrls: ['./content.component.scss'],
    templateUrl: './content.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListFieldComponent,
        ContentStatusComponent,
        DropdownMenuComponent,
        ExternalLinkDirective,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TabRouterlinkDirective,
        TranslatePipe,
    ],
})
export class ContentComponent {
    @Output()
    public clone = new EventEmitter();

    @Output()
    public delete = new EventEmitter();

    @Output()
    public statusChange = new EventEmitter<string>();

    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input({ transform: booleanAttribute })
    public selected = false;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public schema?: SchemaDto;

    @Input()
    public tableFields!: ReadonlyArray<TableField>;

    @Input()
    public tableSettings!: TableSettings;

    @Input({ transform: booleanAttribute })
    public cloneable?: boolean | null;

    @Input()
    public link: any = null;

    @Input('sqxContent')
    public content!: ContentDto;

    @ViewChildren(ContentListFieldComponent)
    public fields!: QueryList<ContentListFieldComponent>;

    public patchForm?: PatchContentForm;
    public patchAllowed?: boolean | null;

    public dropdown = new ModalModel();

    public get isDirty() {
        return this.patchForm?.form.dirty === true;
    }

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsState: ContentsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.content) {
            this.patchAllowed = this.content.canUpdate;
        }

        if (this.patchAllowed && (changes.tableFields || changes.language)) {
            this.patchForm = new PatchContentForm(this.tableFields, this.language);
        }
    }

    public save() {
        if (!this.content.canUpdate || !this.patchForm) {
            return;
        }

        const value = this.patchForm.submit();
        if (!value) {
            return;
        }

        this.contentsState.patch(this.content, value)
            .subscribe({
                next: () => {
                    this.patchForm!.submitCompleted({ noReset: true });

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    this.patchForm!.submitFailed(error);

                    this.changeDetector.detectChanges();
                },
            });
    }

    public shouldStop(field: TableField) {
        if (field.rootField) {
            return this.isDirty || (field.rootField.isInlineEditable && this.patchAllowed);
        } else {
            return this.isDirty;
        }
    }

    public cancel() {
        this.patchForm?.submitCompleted();

        this.fields.forEach(x => x.reset());
    }
}
