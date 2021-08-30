/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ModalModel, StatefulControlComponent, Types } from '@app/framework';
import { AppsState, AssetsService, ROOT_ITEM } from '@app/shared/internal';
import { AssetFolderDropdowNode } from './asset-folder-dropdown.state';

export const SQX_ASSETS_FOLDER_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetFolderDropdownComponent), multi: true,
};

@Component({
    selector: 'sqx-asset-folder-dropdown',
    styleUrls: ['./asset-folder-dropdown.component.scss'],
    templateUrl: './asset-folder-dropdown.component.html',
    providers: [
        SQX_ASSETS_FOLDER_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
})
export class AssetFolderDropdownComponent extends StatefulControlComponent<any, string> {
    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public get appName() {
        return this.appsState.appName;
    }

    public root: AssetFolderDropdowNode = { item: ROOT_ITEM, children: [], parent: null };

    public selection = this.root;
    public selectionPath: string;

    public dropdown = new ModalModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
    ) {
        super(changeDetector, {});
    }

    public writeValue(obj: string) {
        if (!Types.isString(obj)) {
            obj = ROOT_ITEM.id;
        }

        const node = this.findNode(this.root, obj);

        if (node?.isLoaded) {
            this.select(node, false);
            return;
        }

        this.assetsService.getAssetFolders(this.appName, obj, 'PathAndItems')
            .subscribe(dto => {
                let parent = this.root;

                for (const item of dto.path) {
                    let newParent = parent.children.find(x => x.item.id === item.id);

                    if (!newParent) {
                        newParent = { item, children: [], parent };
                        parent.children.push(newParent);
                        parent.children.sortByString(x => x.item.folderName);
                    }

                    parent = newParent;
                }

                if (dto.items.length > 0) {
                    for (const item of dto.items) {
                        if (!parent.children.find(x => x.item.id === item.id)) {
                            parent.children.push({ item, children: [], parent });
                        }
                    }

                    parent.children.sortByString(x => x.item.folderName);
                }

                this.select(parent, false);
            });
    }

    public select(selected: AssetFolderDropdowNode, emit = true) {
        this.resetSelected(this.root);

        const path: AssetFolderDropdowNode[] = [];

        let current: AssetFolderDropdowNode | null = selected.parent;

        while (current) {
            path.push(current);

            current.isExpanded = true;
            current = current.parent;
        }

        this.selection = selected;
        this.selection.isSelected = true;
        this.selectionPath = path.filter(x => x.item !== ROOT_ITEM).map(x => x.item.folderName).join('/');

        if (emit) {
            this.callChange(selected.item.id);
            this.callTouched();

            this.dropdown.hide();
        }
    }

    private resetSelected(node: AssetFolderDropdowNode) {
        node.isSelected = false;

        for (const child of node.children) {
            this.resetSelected(child);
        }
    }

    private findNode(node: AssetFolderDropdowNode, id: string) {
        if (node.item.id === id) {
            return node;
        }

        for (const child of node.children) {
            if (this.findNode(child, id)) {
                return child;
            }
        }

        return undefined;
    }
}
