/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsService,
    DialogModel,
    ImmutableArray,
    MathHelper,
    SchemaDetailsDto,
    SchemasService,
    StatefulControlComponent,
    Types
} from '@app/shared';

export const SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesEditorComponent), multi: true
};

interface State {
    schema?: SchemaDetailsDto | null;
    schemaInvalid: boolean;

    contentItems: ImmutableArray<ContentDto>;
}

@Component({
    selector: 'sqx-references-editor',
    styleUrls: ['./references-editor.component.scss'],
    templateUrl: './references-editor.component.html',
    providers: [SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesEditorComponent extends StatefulControlComponent<State, string[]> implements OnInit {
    @Input()
    public schemaId: string;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    @Input()
    public isCompact = false;

    public selectorDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly schemasService: SchemasService
    ) {
        super(changeDetector, {
            schemaInvalid: false,
            schema: null,
            contentItems: ImmutableArray.empty()
        });
    }

    public ngOnInit() {
        if (this.schemaId === MathHelper.EMPTY_GUID) {
            this.next(s => ({ ...s, schemaInvalid: true }));
            return;
        }

        this.schemasService.getSchema(this.appsState.appName, this.schemaId)
            .subscribe(schema => {
                this.next(s => ({ ...s, schema }));
            }, () => {
                this.next(s => ({ ...s, schemaInvalid: true }));
            });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.isEquals(obj, this.snapshot.contentItems.map(x => x.id).values)) {
                const contentIds: string[] = obj;

                this.contentsService.getContents(this.appsState.appName, this.schemaId, 10000, 0, undefined, contentIds)
                    .subscribe(dtos => {
                        this.setContentItems(ImmutableArray.of(contentIds.map(id => dtos.items.find(c => c.id === id)!).filter(r => !!r)));

                        if (this.snapshot.contentItems.length !== contentIds.length) {
                            this.updateValue();
                        }
                    }, () => {
                        this.setContentItems(ImmutableArray.empty());
                    });
            }
        } else {
            this.setContentItems(ImmutableArray.empty());
        }
    }

    public setContentItems(contentItems: ImmutableArray<ContentDto>) {
        this.next(s => ({ ...s, contentItems }));
    }

    public select(contents: ContentDto[]) {
        for (let content of contents) {
            this.setContentItems(this.snapshot.contentItems.push(content));
        }

        if (contents.length > 0) {
            this.updateValue();
        }

        this.selectorDialog.hide();
    }

    public remove(content: ContentDto) {
        if (content) {
            this.setContentItems(this.snapshot.contentItems.remove(content));

            this.updateValue();
        }
    }

    public sort(contents: ContentDto[]) {
        if (contents) {
            this.setContentItems(ImmutableArray.of(contents));

            this.updateValue();
        }
    }

    private updateValue() {
        let ids: string[] | null = this.snapshot.contentItems.values.map(x => x.id);

        if (ids.length === 0) {
            ids = null;
        }

        this.callTouched();
        this.callChange(ids);
    }
}