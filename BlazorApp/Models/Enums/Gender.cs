using Attributes;

namespace Enums;

enum Gender
{
    [Color("info"), Symbol("♂"), DisplayText("Erkek")]
    Male = 0,
    [Color("danger"), Symbol("♀"), DisplayText("Kadın")]
    Female = 1,
}
