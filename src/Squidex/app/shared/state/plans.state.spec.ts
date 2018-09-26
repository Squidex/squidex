/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    AuthService,
    DialogService,
    PlanChangedDto,
    PlanDto,
    PlansDto,
    PlansService,
    PlansState,
    Version,
    Versioned
} from '@app/shared';

describe('PlansState', () => {
    const app = 'my-app';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldPlans =
        new PlansDto('id1', 'id2', true, [
            new PlanDto('id1', 'name1', '100€', 'id1_yearly', '200€', 1, 1, 1),
            new PlanDto('id2', 'name2', '400€', 'id2_yearly', '800€', 2, 2, 2)
        ],
        version);

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let authService: IMock<AuthService>;
    let plansService: IMock<PlansService>;
    let plansState: PlansState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: 'id3' });

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        plansService = Mock.ofType<PlansService>();

        plansService.setup(x => x.getPlans(app))
            .returns(() => of(oldPlans));

        plansState = new PlansState(appsState.object, authService.object, dialogs.object, plansService.object);
    });

    it('should load plans', () => {
        plansState.load().pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: true,  isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeFalsy();
        expect(plansState.snapshot.isLoaded).toBeTruthy();
        expect(plansState.snapshot.hasPortal).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should load plans with overriden id', () => {
        plansState.load(false, 'id2_yearly').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: true,  plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeFalsy();
        expect(plansState.snapshot.isLoaded).toBeTruthy();
        expect(plansState.snapshot.hasPortal).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        plansState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should redirect when returning url', () => {
        plansState.window = <any>{ location: {} };

        plansService.setup(x => x.putPlan(app, It.isAny(), version))
            .returns(() => of(new Versioned<PlanChangedDto>(newVersion, new PlanChangedDto('URI'))));

        plansState.load().subscribe();
        plansState.change('free').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: true,  isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] }
        ]);
        expect(plansState.window.location.href).toBe('URI');
        expect(plansState.snapshot.version).toEqual(version);
    });

    it('should update plans when no returning url', () => {
        plansState.window = <any>{ location: {} };

        plansService.setup(x => x.putPlan(app, It.isAny(), version))
            .returns(() => of(new Versioned<PlanChangedDto>(newVersion, new PlanChangedDto(''))));

        plansState.load().subscribe();
        plansState.change('id2_yearly').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: true,  plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(newVersion);
    });
});