/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsService,
    getContentValue,
    MathHelper,
    SchemaDetailsDto,
    SchemasService,
    StatefulControlComponent,
    Types
} from '@app/shared';

export const SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesDropdownComponent), multi: true
};

interface State {
    schema?: SchemaDetailsDto | null;

    contentItems: ContentDto[];
    contentNames: ContentName[];

    selectedItem?: ContentName;
}

type ContentName = { name: string, id?: string };

@Component({
    selector: 'sqx-references-dropdown',
    template: `
        <sqx-dropdown [formControl]="selectionControl" [items]="snapshot.contentNames">
            <ng-template let-content="$implicit" let-context="context">
                <span class="truncate" [innerHTML]="content.name | sqxHighlight:context"></span>
            </ng-template>
        </sqx-dropdown>`,
    styles: [
        '.truncate { min-height: 1.5rem; }'
    ],
    providers: [SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesDropdownComponent extends StatefulControlComponent<State, string[]> implements OnInit {
    private languageField: AppLanguageDto;
    private selectedId: string | undefined;

    @Input()
    public schemaId: string;

    @Input()
    public isRequired = false;

    @Input()
    public set language(value: AppLanguageDto) {
        this.languageField = value;

        this.next(s => ({ ...s, contentNames: this.createContentNames(s.schema, s.contentItems) }));
    }

    public selectionControl = new FormControl('');

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly schemasService: SchemasService
    ) {
        super(changeDetector, {
            schema: null,
            contentItems: [],
            contentNames: []
        });

        this.own(
            this.selectionControl.valueChanges
                .subscribe((value: ContentName) => {
                    if (value && value.id) {
                        this.callTouched();
                        this.callChange([value.id]);
                    } else {
                        this.callTouched();
                        this.callChange([]);
                    }
                }));
    }

    public ngOnInit() {
        if (this.schemaId === MathHelper.EMPTY_GUID) {
            this.selectionControl.disable();
            return;
        }

        this.schemasService.getSchema(this.appsState.appName, this.schemaId).pipe(
                switchMap(schema => {
                    if (schema) {
                        return this.contentsService.getContents(this.appsState.appName, this.schemaId, 100, 0);
                    } else {
                        return throwError('Invalid schema');
                    }
                }, (schema, contents) => ({ schema, contents })))
            .subscribe(({ schema, contents }) => {
                const contentItems = contents.items;
                const contentNames = this.createContentNames(schema, contentItems);

                this.next(s => ({ ...s, schema, contentItems, contentNames }));

                this.selectContent();
            }, () => {
                this.selectionControl.disable();
            });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            this.selectedId = obj[0];

            this.selectContent();
        } else {
            this.selectedId = undefined;

            this.unselectContent();
        }
    }

    private selectContent() {
        this.selectionControl.setValue(this.snapshot.contentNames.find(x => x.id === this.selectedId), { emitEvent: false });
    }

    private unselectContent() {
        this.selectionControl.setValue(undefined, { emitEvent: false });
    }

    private createContentNames(schema: SchemaDetailsDto | undefined | null, contents: ContentDto[]): ContentName[] {
        if (contents.length === 0 || !schema) {
            return [];
        }

        const names = contents.map(content => {
            const name =
                schema.referenceFields
                    .map(f => getContentValue(content, this.languageField, f, false))
                    .map(v => v.formatted)
                    .join(', ');

            return { name, id: content.id };
        });

        if (!this.isRequired) {
            return [{ name: '- No Reference -' }, ...names];
        }

        return names;
    }

    public trackByContent(content: ContentDto) {
        return content.id;
    }
}
