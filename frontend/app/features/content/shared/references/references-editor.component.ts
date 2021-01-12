/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppLanguageDto, AppsState, ContentDto, ContentsService, DialogModel, sorted, StatefulControlComponent, Types } from '@app/shared';

export const SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesEditorComponent), multi: true
};

interface State {
    // The content items to show.
    contentItems: ReadonlyArray<ContentDto>;

    // True, when width less than 600 pixels.
    isCompact?: boolean;
}

@Component({
    selector: 'sqx-references-editor',
    styleUrls: ['./references-editor.component.scss'],
    templateUrl: './references-editor.component.html',
    providers: [
        SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesEditorComponent extends StatefulControlComponent<State, ReadonlyArray<string>> {
    @Input()
    public schemaIds: ReadonlyArray<string>;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public formContext: any;

    @Input()
    public allowDuplicates = true;

    public contentCreatorDialog = new DialogModel();
    public contentSelectorDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService
    ) {
        super(changeDetector, { contentItems: [] });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.equals(obj, this.snapshot.contentItems.map(x => x.id))) {
                const contentIds: string[] = obj;

                this.contentsService.getContentsByIds(this.appsState.appName, contentIds)
                    .subscribe(dtos => {
                        this.setContentItems(contentIds.map(id => dtos.items.find(c => c.id === id)!).filter(r => !!r));

                        if (this.snapshot.contentItems.length !== contentIds.length) {
                            this.updateValue();
                        }
                    }, () => {
                        this.setContentItems([]);
                    });
            }
        } else {
            this.setContentItems([]);
        }
    }

    public setContentItems(contentItems: ReadonlyArray<ContentDto>) {
        this.next({ contentItems });
    }

    public select(contents: ReadonlyArray<ContentDto>) {
        this.setContentItems([...this.snapshot.contentItems, ...contents]);

        if (contents.length > 0) {
            this.updateValue();
        }

        this.contentSelectorDialog.hide();
        this.contentCreatorDialog.hide();
    }

    public remove(content: ContentDto) {
        if (content && !this.snapshot.isDisabled) {
            this.setContentItems(this.snapshot.contentItems.filter(x => x.id !== content.id));

            this.updateValue();
        }
    }

    public sort(event: CdkDragDrop<ReadonlyArray<ContentDto>>) {
        if (event && !this.snapshot.isDisabled) {
            this.setContentItems(sorted(event));

            this.updateValue();
        }
    }

    private updateValue() {
        const ids = this.snapshot.contentItems.map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }

    public setCompact(isCompact: boolean) {
        this.next({ isCompact });
    }

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}