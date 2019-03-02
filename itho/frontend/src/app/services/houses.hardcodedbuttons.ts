import { IthoButton } from '../models/itho-button';

export const Wmt6Buttons: IthoButton[] = [
    new IthoButton('Eco',               'main', 'eco'),
    new IthoButton('Comfort',           'main', 'comfort'),
    new IthoButton('Keuken 30 min',     'main', 'cook1'),
    new IthoButton('Keuken 60 min',     'main', 'cook2'),
    new IthoButton('WC beneden 10 min', 'second', 's_timer1'),
    new IthoButton('WC beneden 20 min', 'second', 's_timer2'),
    new IthoButton('WC auto',           'second', 's_auto')
];

export const Wmt40Buttons: IthoButton[] = [
    new IthoButton('Eco',               'main', 'eco'),
    new IthoButton('Comfort',           'main', 'comfort'),
    new IthoButton('Keuken 30 min',     'main', 'cook1'),
    new IthoButton('Keuken 60 min',     'main', 'cook2'),
    new IthoButton('Douche 10 min', 'second', 's_timer1'),
    new IthoButton('Douche 20 min', 'second', 's_timer2')
];

