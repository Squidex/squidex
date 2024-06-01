/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectorRef, Component, EventEmitter, Input, numberAttribute, Output } from '@angular/core';
import { LoaderComponent, StopClickDirective, TranslatePipe } from '@app/framework';
import { AssetsService } from '@app/shared/internal';
import { AssetFolderDropdowNode } from './asset-folder-dropdown.state';

@Component({
    standalone: true,
    selector: 'sqx-asset-folder-dropdown-item',
    styleUrls: ['./asset-folder-dropdown-item.component.scss'],
    templateUrl: './asset-folder-dropdown-item.component.html',
    imports: [
        LoaderComponent,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDropdownItemComponent {
    @Input({ required: true })
    public appName!: string;

    @Input({ required: true })
    public nodeModel!: AssetFolderDropdowNode;

    @Input({ transform: numberAttribute })
    public nodeLevel = 0;

    @Output()
    public selectNode = new EventEmitter<AssetFolderDropdowNode>();

    public get paddingLeft() {
        return `${this.nodeLevel}rem`;
    }

    constructor(
        private readonly assetsService: AssetsService,
        private readonly changeDetector: ChangeDetectorRef,
    ) {
    }

    public toggle() {
        if (this.nodeModel.isExpanded && this.nodeModel.isLoaded) {
            this.collapse();
        } else {
            this.expand();
        }
    }

    public collapse() {
        this.nodeModel.isExpanded = false;
    }

    public expand() {
        this.nodeModel.isExpanded = true;

        this.loadChildren();
    }

    public loadChildren() {
        if (this.nodeModel.isLoading || this.nodeModel.isLoaded) {
            return;
        }

        this.nodeModel.isLoading = true;

        this.assetsService.getAssetFolders(this.appName, this.nodeModel.item.id, 'Items')
            .subscribe({
                next: dto => {
                    if (dto.items.length > 0) {
                        const parent = this.nodeModel;

                        for (const item of dto.items) {
                            if (!parent.children.find(x => x.item.id === item.id)) {
                                parent.children.push({ item, children: [], parent });
                            }
                        }

                        parent.children.sortByString(x => x.item.folderName);
                    }

                    this.nodeModel.isLoaded = true;
                    this.changeDetector.detectChanges();
                },
                complete: () => {
                    setTimeout(() => {
                        this.nodeModel.isLoading = false;
                        this.changeDetector.detectChanges();
                    }, 250);
                },
            });
    }
}
