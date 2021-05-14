/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MathHelper, StatefulControlComponent, value$ } from '@app/framework';
import { AssetPathItem, AssetsService } from '@app/shared/internal';
import { AppsState } from '@app/shared/state/apps.state';
import { ROOT_ITEM } from '@app/shared/state/assets.state';

export const SQX_ASSETS_FOLDER_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetFolderDropdownComponent), multi: true,
};

interface State {
    // The asset folders.
    assetFolders: ReadonlyArray<AssetPathItem>;
}

@Component({
    selector: 'sqx-asset-folder-dropdown',
    styleUrls: ['./asset-folder-dropdown.component.scss'],
    templateUrl: './asset-folder-dropdown.component.html',
    providers: [
        SQX_ASSETS_FOLDER_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetFolderDropdownComponent extends StatefulControlComponent<State, any> implements OnInit {
    @Input()
    public set disabled(value: boolean | null | undefined) {
        this.setDisabledState(value === true);
    }

    public control = new FormControl();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
    ) {
        super(changeDetector, {
            assetFolders: [],
        });

        this.own(
            value$(this.control)
                .subscribe((value: any) => {
                    if (this.control.enabled) {
                        this.callChange(value);
                        this.callTouched();
                    }
                }));
    }

    public ngOnInit() {
        this.assetsService.getAssetFolders(this.appsState.appName, MathHelper.EMPTY_GUID)
            .subscribe(dto => {
                const assetFolders = [ROOT_ITEM, ...dto.items];

                this.next({ assetFolders });
            });
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.control.disable({ emitEvent: false });
        } else {
            this.control.enable({ emitEvent: false });
        }
    }

    public writeValue(obj: any): void {
        this.control.setValue(obj || ROOT_ITEM.id);
    }
}
