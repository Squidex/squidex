/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectorRef, Component, forwardRef, signal } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ModalDirective, ModalModel, ModalPlacementDirective, SignalControlComponent, TranslatePipe, Types } from '@app/framework';
import { AppsState, AssetsService, ROOT_ITEM } from '@app/shared/internal';
import { AssetFolderDropdownItemComponent } from './asset-folder-dropdown-item.component';
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
    imports: [
        AssetFolderDropdownItemComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDropdownComponent extends SignalControlComponent<string> {
    public readonly appName: string;

    public readonly root: AssetFolderDropdowNode = { item: ROOT_ITEM, children: [], parent: null };

    public readonly selection = signal(this.root);
    public readonly selectionPath = signal('');

    public readonly dropdown = new ModalModel();

    constructor(appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly changeDetector: ChangeDetectorRef,
    ) {
        super();

        this.appName = appsState.appName;
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

        selected.isSelected = true;

        this.selection.set(selected);
        this.selectionPath.set(path.filter(x => x.item !== ROOT_ITEM).map(x => x.item.folderName).join('/'));

        if (emit) {
            this.callChange(selected.item.id);
            this.callTouched();

            this.dropdown.hide();
        }

        this.changeDetector.markForCheck();
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
