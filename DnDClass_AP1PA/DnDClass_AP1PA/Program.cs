Character karel = new Character("Karel", 100, Race.Dwarf, Profession.Fighter);
Console.WriteLine(karel.GetAttribute(Attribute.Wisdom));
Console.WriteLine(karel.AttributeRoll(Attribute.Wisdom));

Character pepa = new Character("Pepa", 100, Race.Human, Profession.Ranger);

Weapon turboSword = new Weapon(Attribute.Strength, "TurboSword", 3, 3, Die.D6);
Weapon Bow = new Weapon(Attribute.Dexterity, "Bow", 3, 3, Die.D6);

karel.Pickup(turboSword);
karel.Pickup(turboSword);
karel.CheckBackpack();
karel.Drop("MegaSword");
karel.CheckBackpack();
karel.CheckBackpack();
karel.EquipWeapon("TurboSword");

pepa.Pickup(Bow);
pepa.EquipWeapon("Bow");

int round = 1;
while (true)
{
    Console.WriteLine($"Round {round}!");
    Console.WriteLine($"{karel.Name}: {karel.CurrentHealth}, {pepa.Name}: {pepa.CurrentHealth}");
    if (karel.RollToHit(pepa.AC))
    {
        karel.RollToDamage(pepa);

        if (pepa.CurrentHealth == 0) { break; }

        if (pepa.CurrentHealth <= 15)
        {
            pepa.Cheat();
        }
    }
    if (pepa.RollToHit(karel.AC))
    {
        pepa.RollToDamage(karel);

        if (karel.CurrentHealth == 0) { break; }

        if (karel.CurrentHealth <= 15)
        {
            karel.Cheat();
        }
    }
    round++;
}

class Character
{
    public string Name;
    public int MaxHealth;
    public int CurrentHealth;
    public Race Race;
    public Profession Profession;
    public int[] Attributes;
    public Dictionary<string, Weapon> Backpack;
    public string? CurrentWeaponNameEquipedInHand;
    public int AC;

    public Character(string name, int maxHealth, Race race, Profession profession)
    {
        this.Name = name;
        this.MaxHealth = maxHealth;
        this.CurrentHealth = maxHealth;
        this.Race = race;
        this.Profession = profession;
        this.Backpack = new Dictionary<string, Weapon>();
        this.CurrentWeaponNameEquipedInHand = null;
        switch (race)
        {
            case Race.Human:
                this.Attributes = new int[6] { 3, 3, 3, 3, 3, 3 };
                break;
            case Race.Dwarf:
                this.Attributes = new int[6] { 4, 2, 4, 2, 1, 1 };
                break;
            case Race.Elf:
                this.Attributes = new int[6] { 1, 1, 6, 6, 4, 2 };
                break;
            default:
                this.Attributes = new int[6] { 3, 3, 3, 3, 3, 3 };
                break;
        }

        switch (profession)
        {
            case Profession.Fighter:
                this.Attributes[(int)Attribute.Strength] += 1;
                break;
            case Profession.Ranger:
                this.Attributes[(int)Attribute.Dexterity] += 1;
                break;
            case Profession.Wizard:
                this.Attributes[(int)Attribute.Intelligence] += 1;
                break;
        }

        this.AC = 10 + GetAttribute(Attribute.Dexterity);
    }

    public void SetAttributes(int Strength, int Dexterity, int Constitution, int Intelligence, int Wisdom, int Charisma)
    {
        this.Attributes = new int[6] { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma };
    }

    public int GetAttribute(Attribute attribute)
    {
        return Attributes[((int)attribute)];
    }

    public void TakeDamage(int Damage)
    {
        this.CurrentHealth = this.CurrentHealth - Damage < 0 ? 0 : this.CurrentHealth - Damage;
        if (this.CurrentHealth <= 0)
        {
            Console.WriteLine($"{this.Name} has died!");
        }
    }

    public void HealDamage(int Heal)
    {
        this.CurrentHealth = this.CurrentHealth + Heal > MaxHealth ? MaxHealth : this.CurrentHealth + Heal;
    }

    public int AttributeRoll(Attribute attribute)
    {
        int result = 0;
        var rand = new Random();
        var Roll = rand.Next(1, 21);
        result = this.Attributes[(int)attribute] + Roll;
        return result;
    }

    public void Pickup(Weapon weapon)
    {
        if (this.Backpack.Count < 3)
        {
            if (this.Backpack.ContainsKey(weapon.name))
            {
                Console.WriteLine("Backpack already contains " + weapon.name + "!");
            }
            else
            {
                this.Backpack.Add(weapon.name, weapon);
                Console.WriteLine(this.Name + " picked up " + weapon.name);
            }
        }
        else
        {
            Console.WriteLine("Backpack is full!");
        }
    }

    public void Drop(string weaponName)
    {
        if (Backpack.ContainsKey(weaponName))
        {
            if (CurrentWeaponNameEquipedInHand == weaponName)
            {
                CurrentWeaponNameEquipedInHand = null;
            }
            Backpack.Remove(weaponName);
            Console.WriteLine(this.Name + " dropped " + weaponName);
        }
        else
        {
            Console.WriteLine(weaponName + " doesnt exist!");
        }
    }
    public void CheckBackpack()
    {
        foreach (var item in Backpack)
        {
            Console.WriteLine(item.Key);
        }
    }
    public void EquipWeapon(string WeaponName)
    {
        if (Backpack.ContainsKey(WeaponName))
        {
            CurrentWeaponNameEquipedInHand = WeaponName;
            Console.WriteLine("You have equiped " + WeaponName);
        }
        else
        {
            Console.WriteLine("You dont have this item in your backpack!");
        }
    }
    public void UnEquipWeapon()
    {
        if (CurrentWeaponNameEquipedInHand != null)
        {
            CurrentWeaponNameEquipedInHand = null;
        }
    }
    public bool RollToHit(int AC)
    {
        int roll = 0;
        if (CurrentWeaponNameEquipedInHand is null)
        {
            roll = AttributeRoll(Attribute.Strength);
            return roll >= AC;
        }

        bool success = Backpack.TryGetValue(CurrentWeaponNameEquipedInHand, out var weapon);
        if (success)
        {
            roll = AttributeRoll(weapon.prefferedAttribute);
            roll += weapon.bonusHit;
            return roll >= AC;
        }
        throw new Exception("Error finding equipped weapon in backpack!");
    }

    public void RollToDamage(Character opponent)
    {
        int result = 0;
        var rand = new Random();
        bool success = Backpack.TryGetValue(CurrentWeaponNameEquipedInHand, out var weapon);
        if (success)
        {
            int dmgDie = (int)weapon.damageDie;
            var Roll = rand.Next(1, dmgDie + 1);
            result = this.Attributes[(int)weapon.prefferedAttribute] + Roll + weapon.bonusDamage;
            opponent.TakeDamage(result);
            Console.WriteLine($"{opponent.Name} recieved {result} damage");
        }
        else
        {
            Console.WriteLine("Weapon not found");
        }
    }

    public void Cheat()
    {
        var rand = new Random();
        var Roll = rand.Next(1, 5);

        if (Roll == 1)
        {
            this.HealDamage((int)this.MaxHealth / 10);
            Console.WriteLine($"{this.Name} cheats.");
        }

    }
}

class Weapon
{
    public Attribute prefferedAttribute = Attribute.Constitution;

    public int bonusHit = 0;
    public int bonusDamage = 0;
    public string name = string.Empty;
    public Die damageDie = Die.D6;

    public Weapon(Attribute attribute, string Name, int Hit, int Damage, Die die)
    {
        this.prefferedAttribute = attribute;
        this.name = Name;
        this.bonusHit = Hit;
        this.bonusDamage = Damage;
        damageDie = die;
    }
}

enum Race
{
    Human,
    Dwarf,
    Elf
}

enum Profession
{
    Ranger,
    Wizard,
    Fighter
}

enum Attribute
{
    Strength = 0,
    Dexterity = 1,
    Constitution = 2,
    Intelligence = 3,
    Wisdom = 4,
    Charisma = 5
}
enum Die
{
    D4 = 4,
    D6 = 6,
    D8 = 8,
}