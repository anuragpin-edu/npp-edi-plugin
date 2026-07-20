import os

def generate_large_x12(output_path, segment_count=10000):
    element_sep = '*'
    comp_sep = '>'
    rep_sep = 'U'
    seg_term = '~'
    version = '00501'
    
    isa_fields = [
        "ISA",
        "00", "          ",
        "00", "          ",
        "ZZ", "SENDER         ",
        "ZZ", "RECEIVER       ",
        "260720", "1000",
        rep_sep, version, "000000001", "0", "T", ""
    ]
    isa = element_sep.join(isa_fields)
    isa = isa + comp_sep + seg_term
    
    with open(output_path, 'w', newline='') as f:
        f.write(isa + '\n')
        f.write(f"GS*PO*SENDER*RECEIVER*20260720*1000*1*X*005010{seg_term}\n")
        f.write(f"ST*850*0001{seg_term}\n")
        f.write(f"BEG*00*SA*1001**20260720{seg_term}\n")
        
        # Write PO1 segments until we reach roughly the desired segment count
        for i in range(1, segment_count - 6):
            f.write(f"PO1*{i}*100*EA*10.00**VN*PRODUCT{i}{seg_term}\n")
            
        f.write(f"SE*{segment_count - 3}*0001{seg_term}\n")
        f.write(f"GE*1*1{seg_term}\n")
        f.write(f"IEA*1*000000001{seg_term}\n")

def generate_large_edifact(output_path, segment_count=30000):
    with open(output_path, 'w', newline='') as f:
        f.write("UNA:+.? '\n")
        f.write("UNB+UNOA:1+SENDER+RECEIVER+260720:1000+1'\n")
        f.write("UNH+1+ORDERS:D:96A:UN'\n")
        f.write("BGM+220+1001+9'\n")
        f.write("DTM+137:20260720:102'\n")
        f.write("NAD+BY+BUYER1::92'\n")
        
        for i in range(1, segment_count - 8):
            f.write(f"LIN+{i}++PRODUCT{i}:IN'\n")
            
        f.write(f"UNT*{segment_count - 4}*1'\n")
        f.write("UNZ+1+1'\n")

if __name__ == "__main__":
    script_dir = os.path.dirname(os.path.abspath(__file__))
    print("Generating large X12 sample...")
    generate_large_x12(os.path.join(script_dir, "x12_large.edi"), 10000)
    print("Generating large EDIFACT sample...")
    generate_large_edifact(os.path.join(script_dir, "edifact_large.edi"), 30000)
    print("Done.")
