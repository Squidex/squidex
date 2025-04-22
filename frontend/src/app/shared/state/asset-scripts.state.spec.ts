/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { customMatchers } from 'src/spec/matchers';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, UpdateAssetScriptsDto, versioned } from '@app/shared/internal';
import { AppsService } from '../services/apps.service';
import { createAssetScripts } from '../services/apps.service.spec';
import { TestValues } from './_test-helpers';
import { AssetScriptsState } from './asset-scripts.state';

describe('AssetScriptsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

    const oldScripts = createAssetScripts(1);

    let dialogs: IMock<DialogService>;
    let appsService: IMock<AppsService>;
    let assetScriptsState: AssetScriptsState;

    beforeAll(function () {
        jasmine.addMatchers(customMatchers);
    });

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsService = Mock.ofType<AppsService>();
        assetScriptsState = new AssetScriptsState(appsState.object, appsService.object, dialogs.object);
    });

    afterEach(() => {
        appsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load clients', () => {
            appsService.setup(x => x.getAssetScripts(app))
                .returns(() => of(versioned(version, oldScripts))).verifiable();

            assetScriptsState.load().subscribe();

            expect(assetScriptsState.snapshot.scripts).toEqualIgnoringProps({ query: oldScripts.query, queryPre: oldScripts.queryPre });
            expect(assetScriptsState.snapshot.isLoaded).toBeTruthy();
            expect(assetScriptsState.snapshot.isLoading).toBeFalsy();
            expect(assetScriptsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            appsService.setup(x => x.getAssetScripts(app))
                .returns(() => throwError(() => 'Service Error'));

            assetScriptsState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(assetScriptsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            appsService.setup(x => x.getAssetScripts(app))
                .returns(() => of(versioned(version, oldScripts))).verifiable();

            assetScriptsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            appsService.setup(x => x.getAssetScripts(app))
                .returns(() => of(versioned(version, oldScripts))).verifiable();

            assetScriptsState.load().subscribe();
        });

        it('should update scripts if scripts updated', () => {
            const updated = createAssetScripts(1, '_new');

            const request = new UpdateAssetScriptsDto({ query: 'id3' });

            appsService.setup(x => x.putAssetScripts(app, oldScripts, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            assetScriptsState.update(request).subscribe();

            expect(assetScriptsState.snapshot.scripts).toEqualIgnoringProps({ query: updated.query, queryPre: updated.queryPre });
            expect(assetScriptsState.snapshot.version).toEqual(newVersion);
        });
    });
});
