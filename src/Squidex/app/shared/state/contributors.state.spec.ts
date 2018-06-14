/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AppsState,
    AuthService,
    ContributorsState,
    DialogService,
    Version,
    Versioned
} from '@app/shared';

describe('ContributorsState', () => {
    const app = 'my-app';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldContributors = [
        new AppContributorDto('id1', 'Developer'),
        new AppContributorDto('id2', 'Developer')
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let authService: IMock<AuthService>;
    let contributorsService: IMock<AppContributorsService>;
    let contributorsState: ContributorsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: 'id2' });

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        contributorsService = Mock.ofType<AppContributorsService>();

        contributorsService.setup(x => x.getContributors(app))
            .returns(() => of(new AppContributorsDto(oldContributors, 3, version)));

        contributorsState = new ContributorsState(contributorsService.object, appsState.object, authService.object, dialogs.object);
        contributorsState.load().subscribe();
    });

    it('should load contributors', () => {
        expect(contributorsState.snapshot.contributors.values).toEqual([
            { isCurrentUser: false, contributor: oldContributors[0] },
            { isCurrentUser: true,  contributor: oldContributors[1] }
        ]);
        expect(contributorsState.snapshot.isMaxReached).toBeFalsy();
        expect(contributorsState.snapshot.isLoaded).toBeTruthy();
        expect(contributorsState.snapshot.maxContributors).toBe(3);
        expect(contributorsState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        contributorsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add contributor to snapshot when assigned', () => {
        const newContributor = new AppContributorDto('id3', 'Developer');

        const request = new AppContributorDto('mail2stehle@gmail.com', 'Developer');

        contributorsService.setup(x => x.postContributor(app, request, version))
            .returns(() => of(new Versioned<AppContributorDto>(newVersion, newContributor)));

        contributorsState.assign(request).subscribe();

        expect(contributorsState.snapshot.contributors.values).toEqual([
            { isCurrentUser: false, contributor: oldContributors[0] },
            { isCurrentUser: true,  contributor: oldContributors[1] },
            { isCurrentUser: false,  contributor: newContributor }
        ]);
        expect(contributorsState.snapshot.isMaxReached).toBeTruthy();
        expect(contributorsState.snapshot.maxContributors).toBe(3);
        expect(contributorsState.snapshot.version).toEqual(newVersion);
    });

    it('should update contributor in snapshot when assigned and already added', () => {
        const newContributor = new AppContributorDto('id2', 'Owner');

        const request = new AppContributorDto('mail2stehle@gmail.com', 'Owner');

        contributorsService.setup(x => x.postContributor(app, request, version))
            .returns(() => of(new Versioned<AppContributorDto>(newVersion, newContributor)));

        contributorsState.assign(request).subscribe();

        expect(contributorsState.snapshot.contributors.values).toEqual([
            { isCurrentUser: false, contributor: oldContributors[0] },
            { isCurrentUser: true,  contributor: newContributor }
        ]);
        expect(contributorsState.snapshot.isMaxReached).toBeFalsy();
        expect(contributorsState.snapshot.maxContributors).toBe(3);
        expect(contributorsState.snapshot.version).toEqual(newVersion);
    });

    it('should remove contributor from snapshot when revoked', () => {
        contributorsService.setup(x => x.deleteContributor(app, oldContributors[0].contributorId, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        contributorsState.revoke(oldContributors[0]).subscribe();

        expect(contributorsState.snapshot.contributors.values).toEqual([
            { isCurrentUser: true, contributor: oldContributors[1] }
        ]);
        expect(contributorsState.snapshot.version).toEqual(newVersion);
    });
});